using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using FilterIcap.Net.Exceptions;
using System.IO;

namespace FilterIcap.Net
{
    /// <summary>
    /// This is called whenever an ICAP request message is received from a client.
    /// </summary>
    public delegate void IcapRequestMessageHandler(IcapClient client, IcapRequestMessage message);

    // TODO: Add an event 'OnListenerException' to allow us to restart the listener if the exception is not fatal.
    public class IcapServer
    {
        /// <summary>
        /// The socket receive buffer size. This is merely the largest block we read at once from the socket.
        /// The handler combines these buffers together.
        /// </summary>
        public const int BufferSize = 1024;

        public Socket ClientSocket { get; set; }
        public Socket ServerSocket { get; set; }

        /// <summary>
        /// The ISTag header that gets included in every response to a client.
        /// The server adds quotes characters for you.
        /// </summary>
        public static string ISTag { get; set; }

        /// <summary>
        /// Set this to true when the server should quit. Each thread will begin cleanup once this is set.
        /// </summary>
        /// <value>Set to <c>true</c> if quitting; otherwise, <c>false</c>.</value>
        public bool Quitting { get; set; }

        /// <summary>
        /// If the listener thread fails, this is set.
        /// </summary>
        /// <value>The listener exception.</value>
        public Exception ListenerException { get; set; }

        /// <summary>
        /// Called whenever an ICAP request message is received.
        /// </summary>
        public event IcapRequestMessageHandler OnRequestMessage;

        private AddressFamily addressFamily;
        private string ipAddress;
        private int port;
        private int backlog;
        private IcapServerConfiguration config;

        private Thread listeningThread;
        private List<Thread> clientThreads;

        /// <summary>
        /// Initializes a new instance of an <see cref="T:FilterIcap.Net.IcapServer"/>.
        /// </summary>
        /// <param name="addressFamily">Tells the server whether it should connect to an IPv4 or IPv6 address.
        /// InterNetwork or InterNetworkV6 are expected but not enforced.
        /// </param>
        /// <param name="ipAddress">The IPv4/IPv6 address to connect to.</param>
        /// <param name="port">The port on a given IP address to connect to.</param>
        /// <param name="ISTag">The initial ISTag header value (unquoted).</param>
        public IcapServer(IcapServerConfiguration config)
        {
            this.addressFamily = config.AddressFamily;
            this.ipAddress = config.IpAddress;
            this.port = config.Port;
            this.config = config;
        }

        /// <summary>
        /// Initializes the server socket.
        /// </summary>
        /// <exception cref="FilterIcap.Net.Exceptions.IcapException">Thrown if the bind was unsuccessful. Check InnerException for more details.</exception>
        public void Init()
        {
            try {
                ServerSocket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
                var endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

                ServerSocket.Blocking = false;

                ServerSocket.Bind(endPoint);

            } catch(Exception ex) {
                var icapEx = new IcapException("Failed to initialize server. Check InnerException for more details.", ex);
                throw icapEx;
            }
        }

        /// <summary>
        /// Starts the listener thread for this Icap server.
        /// </summary>
        /// <param name="backlog">The maximum length of the pending connections queue.</param>
        public void StartListening(int backlog)
        {
            this.backlog = backlog;

            if(listeningThread == null) {
                listeningThread = new Thread(ListenerThread);
                listeningThread.Start();
            }
        }

        private void ClientThread(object socketObj)
        {
            Socket icapSocket = socketObj as Socket;
            IcapClient client = new IcapClient(icapSocket, this);

            IcapState state = IcapState.CollectingData;

            byte[] receiveBuffer;
            List<ByteArrayInfo> buffers = new List<ByteArrayInfo>();

            byte[] dataBuffer = null;
            int recvBytes = 0, totalReceived = 0;

            while (!Quitting)
            {
                if(!client.Socket.Connected) {
                    break;
                }

                switch (state)
                {
                    // FIXME: Don't leave CollectingData state until two CRLFs are received from socket.
                    // Not sure if that's what squid is going to do or not.

                    // FIXME: Handle sockets closed by the remote server.

                    case IcapState.CollectingData:
                        receiveBuffer = new byte[BufferSize];
                        recvBytes = icapSocket.Receive(receiveBuffer);
                        buffers.Add(new ByteArrayInfo(receiveBuffer, recvBytes));

                        if (recvBytes < BufferSize)
                        {
                            totalReceived += recvBytes;

                            dataBuffer = new byte[totalReceived];
                            int itr = 0;
                            foreach (var buf in buffers)
                            {
                                new ArraySegment<byte>(buf.ByteArray, 0, buf.DataLength).CopyTo(dataBuffer, itr);

                                itr += buf.DataLength;
                            }
                            state = IcapState.ParsingData;

                            recvBytes = 0;
                            totalReceived = 0;
                            break;
                        }
                        else if (recvBytes == BufferSize)
                        {
                            totalReceived += BufferSize;
                        }

                        break;

                    case IcapState.ParsingData:

                        try
                        {
                            string messageString = Encoding.ASCII.GetString(dataBuffer);

                            Console.WriteLine($"Request received: {messageString}");

                            Messages.Parsing.StreamParser parser = new Messages.Parsing.StreamParser();

                            IcapRequestMessage message = null;
                            using (MemoryStream stream = new MemoryStream(dataBuffer))
                            {
                                message = parser.Parse(stream);
                            }


                            if (message != null)
                            {
                                if(message.Method == "OPTIONS") {
                                    var response = this.GetConfigurationResponse();

                                    if(response != null) {
                                        client.Send(response);
                                    }
                                } else {
                                    OnRequestMessage?.Invoke(client, message);
                                }
                            }
                            else
                            {
                                Console.WriteLine($"IcapRequestMessage was NULL. messageString = '{messageString}'");
                            }

                            state = IcapState.CollectingData;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            state = IcapState.CollectingData;
                        }

                        break;
                }
            }
        }

        private void ListenerThread()
        {
            try
            {
                ServerSocket.Listen(backlog);

                while (!Quitting)
                {
                    try
                    {
                        Socket clientSocket = ServerSocket.Accept();

                        if (clientSocket != null)
                        {
                            var clientThread = new Thread(ClientThread);
                            clientThread.Start(clientSocket);
                        }
                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode != SocketError.WouldBlock)
                        {
                            throw ex;
                        }
                        else
                        {
                            Thread.Sleep(5);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ListenerException = ex;
            }
        }

        IcapResponseMessage GetConfigurationResponse()
        {
            IcapResponseMessage message = new IcapResponseMessage();

            message.Headers["Methods"] = string.Join(", ", this.config.SupportedMessageTypes);
            message.Headers["Service"] = this.config.Service;
            message.Headers["Encapsulated"] = "null-body=0";

            message.StatusCode = IcapStatusCode.OK;

            return message;
        }
    }
}
