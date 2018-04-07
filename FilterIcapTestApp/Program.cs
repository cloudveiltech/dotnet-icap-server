using System;
using System.IO;
using System.Diagnostics;

using FilterIcap.Net;
using FilterIcap.Net.Messages.Parsing;

namespace FilterIcapTestApp
{
    class Program
    {
        /// <summary>
        /// Use this program to debug ICAP server code.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        static void Main(string[] args)
        {
            RunTest("FilterIcapTestApp.Resources.test-reqmod-no-body.txt");
            RunTest("FilterIcapTestApp.Resources.test-reqmod-req-body.txt");
            Console.ReadKey();
        }

        static IcapRequestMessage RunTest(string name) {
            using (Stream stream = ResourceStream.GetStream(name))
            {
                StreamParser parser = new StreamParser();

                Stopwatch stopwatch = Stopwatch.StartNew();
                IcapRequestMessage message = parser.Parse(stream);
                stopwatch.Stop();

                Console.WriteLine("ICAP parsing performance on {0} is {1}ms", name, stopwatch.ElapsedMilliseconds);

                return message;
            }
        }
    }
}
