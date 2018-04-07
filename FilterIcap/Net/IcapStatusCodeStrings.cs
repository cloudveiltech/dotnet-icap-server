using System;
using System.Collections.Generic;

namespace FilterIcap.Net
{
    public static class IcapStatusCodeStrings
    {
        static Dictionary<IcapStatusCode, string> _strings = new Dictionary<IcapStatusCode, string>() {
            { IcapStatusCode.OK, "OK" }
        };

        public static Dictionary<IcapStatusCode, string> Strings
        {
            get { return _strings; }
        }

        public static string Get(IcapStatusCode code)
        {
            string value = null;

            if(_strings.TryGetValue(code, out value)) {
                return value;
            } else {
                return "Unknown Code";
            }
        }
    }
}
