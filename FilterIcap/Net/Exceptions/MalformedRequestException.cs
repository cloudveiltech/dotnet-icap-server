/*
* Copyright © 2018 Cloudveil Technology Inc.  
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

using System;
namespace FilterIcap.Net.Exceptions
{
    /// <summary>
    /// Thrown typically when the request sent by the ICAP client was not what we expected.
    /// </summary>
    public class MalformedRequestException : IcapException
    {
        /// <summary>
        /// Raw string as received from ICAP client. This may be NULL.
        /// </summary>
        /// <value>The entire ICAP message as received from the client.</value>
        public string RawRequest { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:FilterIcap.Net.Exceptions.MalformedRequestException"/> class.
        /// </summary>
        /// <param name="message">The (hopefully) user-friendly message of what went wrong and why we threw this error.</param>
        public MalformedRequestException(string message) : base(message)
        {
            RawRequest = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:FilterIcap.Net.Exceptions.MalformedRequestException"/> class.
        /// </summary>
        /// <param name="message">The (hopefully) user-friendly message of what went wrong and why we threw this error.</param>
        /// <param name="rawRequest">The raw request string, which sometimes turns out to be useful for debugging purposes.</param>
        public MalformedRequestException(string message, string rawRequest) : base(message)
        {
            RawRequest = rawRequest;
        }
    }
}
