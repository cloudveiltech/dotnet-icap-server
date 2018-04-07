using System;
using System.IO;
using Xunit;
using FilterIcap.Net.Messages.Parsing;
using FilterIcap.Net;

namespace FilterIcapTests
{
    public class StreamParserTests
    {
        [Fact]
        public void EmptyReqModParserTest()
        {
            using (Stream stream = ResourceStream.GetStream("Resources.test-reqmod-no-body.txt"))
            {
                Assert.NotNull(stream);

                StreamParser parser = new StreamParser();

                IcapRequestMessage message = parser.Parse(stream);

                string hostHeader = message.Headers["Host"];
                Assert.NotNull(hostHeader);

                Assert.Equal(hostHeader, "icap-server.net");
                Assert.Equal(message.Encapsulations.Count, 1);
                Assert.Equal(message.Encapsulations[0].Type, EncapsulationType.RequestHeader);
                Assert.Equal(message.Encapsulations[0].Data.IndexOf("GET / HTTP/1.1"), 0);
            }
        }
    }
}
