using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace FilterIcap.Net
{
    public class IcapServerConfiguration
    {
        public IcapServerConfiguration(AddressFamily af, string ipAddress, int port)
        {
            SupportedMessageTypes = new List<string>();

            AddressFamily = af;
            IpAddress = ipAddress;
            Port = port;
        }

        /// <summary>
        /// Use this to set the supported message types. The available options are "REQMOD" and "RESPMOD".
        /// The ICAP server package handles the "OPTIONS" type for you.
        /// </summary>
        /// <value>The supported message types.</value>
        public List<string> SupportedMessageTypes { get; private set; }

        /// <summary>
        /// This is what the 'Service' header is set to.
        /// </summary>
        /// <value>The service.</value>
        public string Service { get; set; }

        /// <summary>
        /// Gets or sets the IP address. This can be either IPv4 or IPv6. Determined by the selection in AddressFamily.
        /// </summary>
        /// <value>The ip address.</value>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the port. Typically 1344.
        /// </summary>
        /// <value>The port.</value>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the address family. Typically either InterNetwork or InterNetworkV6.
        /// </summary>
        /// <value>The address family.</value>
        public AddressFamily AddressFamily { get; set; }
    }
}
