/*
* Copyright © 2018 Cloudveil Technology Inc.  
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.IO;

using FilterIcap.Net.Exceptions;

namespace FilterIcap.Net
{
    /// <summary>
    /// This is a generic request/response data class for ICAP messages.
    /// It attempts to generalize between both requests and responses.
    /// </summary>
    public class IcapRequestMessage
    {
        static string[] validMethods = { "OPTIONS", "RESPMOD", "REQMOD" };

        /// <summary>
        /// The ICAP method for the particular message.
        /// </summary>
        /// <value></value>
        public string Method { get; set; }

        // FIXME: System.Uri does not support the 'icap://' scheme.
        /// <summary>
        /// The complete URI of the request.
        /// </summary>
        /// <value>The request URI.</value>
        public string RequestUri { get; set; }

        /// <summary>
        /// The header collection from the request.
        /// Keys do not include the ':' part of the header.
        /// </summary>
        /// <value>Contains key-value pairs of headers.</value>
        public NameValueCollection Headers { get; } = new NameValueCollection();

        /// <summary>
        /// The body of the ICAP request message. Includes all CRLF characters.
        /// </summary>
        /// <value>The request body.</value>
        public string RequestBody { get; set; }

        public List<Encapsulation> Encapsulations { get; set; } = new List<Encapsulation>();

        public IcapRequestMessage()
        {
        }


    }
}
