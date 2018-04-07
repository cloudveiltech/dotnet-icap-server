using System;
namespace FilterIcap.Net
{
    public class Encapsulation
    {
        public Encapsulation(string data, EncapsulationType type)
        {
            Data = data;
            Type = type;
        }

        /// <summary>
        /// The encapsulated data fetch from the body.
        /// </summary>
        /// <value>The data.</value>
        public string Data { get; set; }

        /// <summary>
        /// The type of encapsulated data that was parsed out.
        /// </summary>
        /// <value>The type.</value>
        public EncapsulationType Type { get; set; }
    }
}
