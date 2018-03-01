/*
* Copyright © 2018 Cloudveil Technology Inc.  
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/.
*/

using System;
using System.Text;
using FilterIcap.Net.Exceptions;

namespace FilterIcap.Net
{
    /// <summary>
    /// This is a generic request/response data class for ICAP messages.
    /// It attempts to generalize between both requests and responses.
    /// </summary>
    public class IcapRequestMessage
    {
        private static string[] validMethods = new string[] { "OPTIONS", "RESPMOD", "REQMOD" };

        /// <summary>
        /// The ICAP method for the particular message.
        /// </summary>
        /// <value></value>
        public string Method { get; set; }

        /// <summary>
        /// The complete URI of the request.
        /// </summary>
        /// <value>The request URI.</value>
        public Uri RequestUri { get; set; }

        public NameValueCollection
        public IcapRequestMessage()
        {
        }

        private enum IcapParsingState
        {
            /// <summary>
            /// This tells the state-machine that the current line is a request line.
            /// This state can only happen once per request.
            /// </summary>
            RequestLine,
            HeaderCollect,
            HeaderContinue,
            BodyCollect,
            Unknown
        }

        public static IcapRequestMessage Parse(string messageString)
        {
            string[] lines = messageString.Split("\r\n");

            IcapRequestMessage message = new IcapRequestMessage();

            IcapParsingState icapParsingState = IcapParsingState.Unknown;
            bool hasRequestLine = false;

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < lines.Length; i++)
            {
                if (i > 0 && !hasRequestLine)
                {
                    throw new MalformedRequestException($"Malformed request. No request line was detected.", messageString);
                }

                // This strikes me as a very odd way to begin parsing a request.
                // But we'll try this anyway and see if it works.

                // If we have ICAP/1.0 in the line, we have a valid request line.
                if (lines[i].IndexOf("ICAP/1.0", StringComparison.OrdinalIgnoreCase) > 0)
                {
                    icapParsingState = IcapParsingState.RequestLine;
                    hasRequestLine = true;
                }
                else if (!hasRequestLine)
                {
                    // hasRequestLine should be true by the time this gets run.
                    // The only time this will get run is when i==0
                    throw new MalformedRequestException($"Malformed request. Request line was malformed. '{lines[i]}'", messageString);
                }

                switch (icapParsingState)
                {
                    case IcapParsingState.RequestLine:
                        // First token in request line is the method.
                        // Second is URI.
                        // Third is ICAP/1.0
                        string[] requestLineTokens = lines[i].Split(' ');
                        if (!isValidMethod(requestLineTokens[0]))
                        {
                            throw new MalformedRequestException($"Malformed request. Invalid method for ICAP.", messageString);
                        }

                        message.Method = requestLineTokens[0];

                        // There may be an ICAP client which tries to insert a space in the URI, but for now we'll pretend like those clients don't exist.
                        message.RequestUri = new Uri(requestLineTokens[1]);

                        break;
                }
            }

            if (string.IsNullOrWhiteSpace(requestLine))
            {
                throw new MalformedRequestException($"Malformed request. '{requestLine}' is malformed.", messageString);
            }

            // First line of the request should be something like
            // OPTIONS icap://127.0.0.1:1344/request ICAP/1.0
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that contains the text of the request/response message represented by this class.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/></returns>
		public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            return builder.ToString();
        }

        private static bool isValidMethod(string method)
        {
            for (int i = 0; i < validMethods.Length; i++)
            {
                if (method.Equals(method, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
