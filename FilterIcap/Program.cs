/*
* Copyright © 2018 Cloudveil Technology Inc.  
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using FilterIcap.Net;

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
        static void Main(string[] args)
        {
            IcapServer.ISTag = "_cloudveil_icap_0.1";

            IcapServerConfiguration config = new IcapServerConfiguration(AddressFamily.InterNetwork, "127.0.0.1", 1344);

            config.SupportedMessageTypes.Add("REQMOD");

            // Only do redirects on requests of known bad sites.
            //config.SupportedMessageTypes.Add("RESPMOD");
            config.Service = "CloudVeil Technology dotnet-icap-server";

            IcapServer server = new IcapServer(config);
            server.Init();
            server.StartListening(512);

            server.OnRequestMessage += OnRequestMessage;

            while(true) {
                Thread.Sleep(50);
            }
        }

        // TODO: The server needs to handle the options request.
        // TODO: We need a server configuration class.

        static void OnRequestMessage(IcapClient client, IcapRequestMessage request)
        {
            IcapResponseMessage response = null;

            switch(request.Method) {
                case "REQMOD":
                    
                    // TODO: Build a reqmod that blocks youtube.
                    // TODO: Also after lunch, start thinking about how to integrate this with Filter-Windows.
                    break;

                case "RESPMOD":
                    break;
            }

            if(response != null) {
                client.Send(response);
            }
        }
    }
}
