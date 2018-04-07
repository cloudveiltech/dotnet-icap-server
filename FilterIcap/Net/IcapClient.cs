using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FilterIcap.Net
{
    /// <summary>
    /// Used to store information about a particular client connection.
    /// </summary>
    public class IcapClient
    {
        public Socket Socket { get; set; }
        public IcapServer Server { get; set; }

        public IcapClient(Socket socket, IcapServer server)
        {
            Socket = socket;
            Server = server;
        }

        public void Send(IcapResponseMessage message)
        {
            string messageString = message.ToString();
            Console.WriteLine($"Response sent: {messageString}");

            byte[] messageBytes = Encoding.ASCII.GetBytes(messageString);

            Socket.Send(messageBytes);
        }
    }
}
