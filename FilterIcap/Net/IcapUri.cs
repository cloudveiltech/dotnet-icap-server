using System;
namespace FilterIcap.Net
{
    public class IcapUri
    {
        private Uri internalUri;

        public IcapUri(string uriString)
        {
            string httpUri = uriString.Replace("icap://", "http://");
        }
    }
}
