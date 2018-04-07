using System;
using System.Text;
using System.Collections.Specialized;

namespace FilterIcap.Net
{
    public class IcapResponseMessage
    {
        public IcapStatusCode StatusCode { get; set; }

        public NameValueCollection Headers { get; } = new NameValueCollection();

        public string ResponseBody { get; set; }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:FilterIcap.Net.IcapResponseMessage"/>. Builds a valid response to a particular request.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:FilterIcap.Net.IcapResponseMessage"/>.</returns>
		public override string ToString()
		{
            StringBuilder builder = new StringBuilder();

            builder.Append($"ICAP/1.0 {(int)StatusCode} {IcapStatusCodeStrings.Get(StatusCode)}\r\n");

            bool includeISTag = true;

            for (int i = 0; i < Headers.AllKeys.Length; i++)
            {
                var key = Headers.AllKeys[i];
                if (key.Equals("ISTag", StringComparison.OrdinalIgnoreCase))
                {
                    // We must include this tag in every response, so we add it down below.
                    continue;
                }

                // FIXME: Build whitespace folding for headers above 998 characters long.
                builder.Append($"{key}: {Headers[key]}\r\n");
            }

            builder.Append($"ISTag: \"{IcapServer.ISTag}\"\r\n");
            builder.Append("\r\n");

            if(ResponseBody != null) {
                // FIXME: Make sure there are no LFs or CRs by themselves.
                builder.Append(ResponseBody);
            }

            builder.Append("\r\n");

            return builder.ToString();
		}
	}
}
