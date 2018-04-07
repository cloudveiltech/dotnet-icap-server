using System;
using System.IO;

namespace FilterIcap.Net.Messages.Parsing
{
    public delegate void RequestLineDelegate(string method, string uri, string version);
    public delegate void HeaderDelegate(string name, string value);
    public delegate void EncapsulatedHeadersDelegate(EncapsulationType type, byte[] httpHeaders);
    public delegate void EncapsulatedBodyChunkDelegate(EncapsulationType type, byte[] bodyChunk);

    public interface IStreamParser
    {

        /// <summary>
        /// This property determines whether the message currently being received is in preview mode. This is set to false when a preview header is received.
        /// </summary>
        /// <value><c>true</c> if is preview; otherwise, <c>false</c>.</value>
        bool IsPreview { get; }

        event RequestLineDelegate OnRequestLine;
        event HeaderDelegate OnHeader;
        event EncapsulatedHeadersDelegate OnEncapsulatedHeaders;
        event EncapsulatedBodyChunkDelegate OnEncapsulatedBodyChunk;

        void Parse(Stream stream);
    }
}
