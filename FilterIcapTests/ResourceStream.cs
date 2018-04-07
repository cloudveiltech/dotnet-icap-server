using System;
using System.IO;
using System.Reflection;

namespace FilterIcapTests
{
    public static class ResourceStream
    {
        public static Stream GetStream(string fileName) {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = fileName;

            Stream stream = assembly.GetManifestResourceStream(resourceName);
            return stream;
        }
    }
}
