using System;
namespace FilterIcap.Net.Messages.Parsing
{
    public enum IcapParsingState
    {
        /// <summary>
        /// This tells the state-machine that the current line is a request line.
        /// This state can only happen once per request.
        /// </summary>
        RequestLine,

        /// <summary>
        /// This tells the state-machine that we are in header-parsing mode.
        /// This handles all header parsing and unfolding.
        /// </summary>
        HeaderCollect,

        /// <summary>
        /// The state-machine goes into this mode when we reach an empty line in HeaderCollect mode.
        /// We remain in this mode until the end of the message.
        /// </summary>
        BodyCollect,

        /// <summary>
        /// The state-machine goes into this state after the Encapsulated header is parsed. Is responsible for breaking out the different encapsulations.
        /// </summary>
        EncapsulatedCollect,

        /// <summary>
        /// The state-machine goes into this state when an encapsulated body is reached. It is responsible for 
        /// </summary>
        EncapsulatedBodyCollect,

        /// <summary>
        /// Notification for the state-machine to stop parsing. We've completed this and got all we can get out of it.
        /// </summary>
        Completed,

        /// <summary>
        /// Used as an initialization value for the state.
        /// </summary>
        Unknown

    }
}
