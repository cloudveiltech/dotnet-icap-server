/*
* Copyright © 2018 Cloudveil Technology Inc.  
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FilterIcap
{
    public enum IcapState {
        CollectingData,
        ParsingData,
        SendingData
    }

    /// <summary>
    /// This class stores information about each buffer and how many bytes are in each one.
    /// </summary>
    public class ByteArrayInfo {
        public ByteArrayInfo(byte[] byteArray, int dataLength) {
            ByteArray = byteArray;
            DataLength = dataLength;
        }
        public byte[] ByteArray { get; set; }
        public int DataLength { get; set; }
    }

    class Program
    {
        static Socket clientSocket;

        static void Main(string[] args)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1344);
            socket.Bind(endPoint);
            socket.Listen(512);

            while(true) {
                Socket client = socket.Accept();
                if(client != null) {
                    Console.WriteLine("Client accepted.");
                    clientSocket = client;

                    break;
                }
            }

            Thread icap = new Thread(IcapThread);
            icap.Start(clientSocket);

            while(true) {
                Thread.Sleep(50);
            }
        }

        const int BufferSize = 1024;

        static void IcapThread(object socketObj) {
            Socket icapSocket = socketObj as Socket;

            IcapState state = IcapState.CollectingData;

            byte[] receiveBuffer;
            List<ByteArrayInfo> buffers = new List<ByteArrayInfo>();

            byte[] dataBuffer = null;
            int recvBytes = 0, totalReceived = 0;

            while(true) {
                switch (state)
                {
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
                        string message = Encoding.ASCII.GetString(dataBuffer);

                        Console.WriteLine(message);
                        break;
                }
            }
        }
    }
}
