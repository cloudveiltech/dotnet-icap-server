using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Net.Sockets;
using FilterIcap.Net.Exceptions;

namespace FilterIcap.Net.Messages.Parsing
{
    public class EncapsulationRange
    {
        public EncapsulationRange(int start, int end)
        {
            Start = start;
            End = end;
        }

        public int Start { get; set; }
        public int End { get; set; }
    }

    public class StreamParser
    {
        static Dictionary<string, EncapsulationType> encapsulationTypes;
        static List<string> validMethods;

        static StreamParser()
        {
            encapsulationTypes = new Dictionary<string, EncapsulationType>();
            validMethods = new List<string>();

            encapsulationTypes.Add("req-hdr", EncapsulationType.RequestHeader);
            encapsulationTypes.Add("req-body", EncapsulationType.RequestBody);
            encapsulationTypes.Add("res-hdr", EncapsulationType.ResponseHeader);
            encapsulationTypes.Add("res-body", EncapsulationType.ResponseBody);
            encapsulationTypes.Add("null-body", EncapsulationType.NullBody);
            encapsulationTypes.Add("opt-body", EncapsulationType.OptionBody);

            validMethods.Add("RESPMOD");
            validMethods.Add("REQMOD");
            validMethods.Add("OPTIONS");
        }

        public IcapRequestMessage Parse(Stream stream)
        {
            IcapRequestMessage message = new IcapRequestMessage();
            bool hasRequestLine = false;

            // This is a body builder. Unfortunately, this one doesn't go to the gym.
            StringBuilder bodyBuilder = new StringBuilder();

            string currentHeaderName = null;
            StringBuilder currentHeaderValue = new StringBuilder();
            IcapParsingState icapParsingState = IcapParsingState.Unknown;

            using (StreamReader reader = new StreamReader(stream))
            {
                string line = reader.ReadLine();

                // If we have ICAP/ in the line, we have a valid request line.
                // FIXME: We need an ICAP version string in IcapRequestMessage
                int indexOfIcap = line.IndexOf("ICAP/", StringComparison.OrdinalIgnoreCase);
                if (indexOfIcap > 0)
                {
                    // First token in request line is the method.
                    // Second is URI.
                    // Third is ICAP/1.0
                    string[] requestLineTokens = line.Split(' ');
                    if (!isValidMethod(requestLineTokens[0]))
                    {
                        throw new MalformedRequestException($"Malformed request. Invalid method for ICAP.");
                    }

                    message.Method = requestLineTokens[0];

                    // There may be an ICAP client which tries to insert a space in the URI, but for now we'll pretend like those clients don't exist.
                    message.RequestUri = requestLineTokens[1];

                    int i = 0;
                    for (i = indexOfIcap + 5; i < line.Length; i++)
                    {
                        if (char.IsWhiteSpace(line[i]))
                        {
                            break;
                        }
                    }

                    string version = line.Substring(indexOfIcap + 5, i - (indexOfIcap + 5));

                    OnRequestLine?.Invoke(message.Method, message.RequestUri, version);

                    icapParsingState = IcapParsingState.HeaderCollect;
                }
                else
                {
                    throw new MalformedRequestException($"Malformed request. Request line was '{line}'");
                }

                bool isEncapsulatedMode = false;
                Dictionary<EncapsulationType, EncapsulationRange> encapsulatedByteIndexes = new Dictionary<EncapsulationType, EncapsulationRange>();

                while ((line = reader.ReadLine()) != null)
                {
                    if(icapParsingState == IcapParsingState.Completed) {
                        break;
                    }

                    if (isEncapsulatedMode && !string.IsNullOrEmpty(line))
                    {
                        icapParsingState = IcapParsingState.HeaderCollect;
                        isEncapsulatedMode = false;
                    }

                    switch(icapParsingState) {
                        case IcapParsingState.HeaderCollect:
                            // Empty line signals start of body.
                            if (line == "")
                            {
                                icapParsingState = IcapParsingState.BodyCollect;

                                // Clean up last header and add it before going into body parsing mode.
                                if (currentHeaderName != null)
                                {
                                    message.Headers[currentHeaderName] = currentHeaderValue.ToString().Trim();
                                    currentHeaderValue.Clear();
                                    currentHeaderName = null;
                                }

                                break;
                            }

                            if (line.Length > 0 && char.IsWhiteSpace(line[0]))
                            {
                                // This is a continuation of a previous line's header value.
                                currentHeaderValue.Append(line);
                            }
                            else
                            {
                                if (currentHeaderName != null)
                                {
                                    message.Headers[currentHeaderName] = currentHeaderValue.ToString().Trim();
                                    currentHeaderValue.Clear();
                                    currentHeaderName = null;
                                }

                                if (line.IndexOf(':') < 0)
                                {
                                    throw new MalformedRequestException($"Expected header name. Got '{line}' instead.");
                                }

                                string[] headerParts = line.Split(':');
                                currentHeaderName = headerParts[0];

                                // Loop through header parts just in case there is more than one ':' character in the line.
                                // I don't believe RFC2822 prohibits that.
                                for (int j = 1; j < headerParts.Length; j++)
                                {
                                    currentHeaderValue.Append(headerParts[j]);

                                    if (j < headerParts.Length - 1)
                                    {
                                        currentHeaderValue.Append(':');
                                    }
                                }

                                if (currentHeaderName.Equals("Encapsulated", StringComparison.OrdinalIgnoreCase))
                                {
                                    string headerInfo = currentHeaderValue.ToString();
                                    string[] headerInfoParts = headerInfo.Split(',');

                                    encapsulatedByteIndexes.Clear();

                                    List<Tuple<EncapsulationType, int>> byteStarts = new List<Tuple<EncapsulationType, int>>();

                                    for (int i = 0; i < headerInfoParts.Length; i++)
                                    {
                                        string[] keyValue = headerInfoParts[i].Split('=');

                                        // Clear out whitespace on 'req-hdr' part.
                                        keyValue[0] = keyValue[0].Trim();
                                        keyValue[1] = keyValue[1].Trim(); // Just for future safety

                                        int byteStart;
                                        if (!int.TryParse(keyValue[1], out byteStart))
                                        {
                                            throw new MalformedRequestException($"Encapsulated {keyValue[0]} value was not a number: value={keyValue[1]}");
                                        }

                                        byteStarts.Add(new Tuple<EncapsulationType, int>(encapsulationTypes[keyValue[0]], byteStart));

                                        //encapsulatedByteIndexes.Add(encapsulationTypes[keyValue[0]], int.Parse(keyValue[1]));
                                        // TODO: Instead of using integers here in this dictionary, use EncapsulationRange instead.
                                    }

                                    Comparison<Tuple<EncapsulationType, int>> comparison = new Comparison<Tuple<EncapsulationType, int>>((a, b) =>
                                    {
                                        if(a.Item2 < b.Item2) {
                                            return -1;
                                        } else if(a.Item2 > b.Item2) {
                                            return 1;
                                        } else {
                                            return 0;
                                        }
                                    });

                                    byteStarts.Sort(comparison);

                                    for (int i = 0; i < byteStarts.Count; i++)
                                    {
                                        int start, end;
                                        start = byteStarts[i].Item2;
                                        end = (i + 1 < byteStarts.Count) ? byteStarts[i + 1].Item2 - 1 : -1;

                                        encapsulatedByteIndexes.Add(byteStarts[i].Item1, new EncapsulationRange(start, end));
                                    }

                                    icapParsingState = IcapParsingState.EncapsulatedCollect;
                                    isEncapsulatedMode = true;
                                }
                            }

                            break;

                        case IcapParsingState.EncapsulatedCollect:
                            foreach (var encapsulationRange in encapsulatedByteIndexes)
                            {
                                int start = encapsulationRange.Value.Start;
                                int length = encapsulationRange.Value.End - start + 1;

                                switch(encapsulationRange.Key) {
                                    case EncapsulationType.NullBody:
                                        icapParsingState = IcapParsingState.HeaderCollect;
                                        isEncapsulatedMode = false;
                                        break;

                                    case EncapsulationType.OptionBody:
                                    case EncapsulationType.RequestBody:
                                    case EncapsulationType.ResponseBody:
                                        icapParsingState = IcapParsingState.EncapsulatedBodyCollect;
                                        break;
                                }

                                if(icapParsingState == IcapParsingState.EncapsulatedBodyCollect) {
                                    break;
                                }

                                if(encapsulationRange.Key == EncapsulationType.NullBody) {
                                    // It's safe to assume that if null-body is received, we don't have to do any
                                    // more encapsulation parsing.
                                    icapParsingState = IcapParsingState.HeaderCollect;
                                    isEncapsulatedMode = false;
                                    break;
                                }

                                if (length > 0)
                                {
                                    char[] buffer = new char[length];
                                    reader.Read(buffer, 0, length);

                                    Encapsulation encap = new Encapsulation(new string(buffer), encapsulationRange.Key);
                                    message.Encapsulations.Add(encap);
                                }
                                else if (encapsulationRange.Value.End == -1)
                                {
                                    // There was no body specified, let's just assume we're done.

                                    string headerLine = null;

                                    while ((headerLine = reader.ReadLine()) != null)
                                    {
                                        if (string.IsNullOrEmpty(headerLine))
                                        {
                                            // Done reading encapsulated headers.
                                            icapParsingState = IcapParsingState.Completed;
                                            break;
                                        }
                                    }
                                }
                            }

                            break;
                    }
                }

                return message;
            }
        }

        public event RequestLineDelegate OnRequestLine;

        static bool isValidMethod(string method)
        {
            for (int i = 0; i < validMethods.Count; i++)
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
