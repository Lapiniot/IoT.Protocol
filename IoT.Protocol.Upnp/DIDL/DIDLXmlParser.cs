using System.Collections.Generic;
using System.IO;
using System.Xml;
using IoT.Protocol.Upnp.DIDL.Readers;
using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Upnp.DIDL
{
    public static class DIDLXmlParser
    {
        private const string Ns = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/";

        private static readonly XmlReaderSettings Settings = new()
        {
            CloseInput = true,
            ConformanceLevel = ConformanceLevel.Fragment,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            IgnoreWhitespace = true
        };

        public static IEnumerable<Item> Parse(string content, bool parseResourceProps, bool parseVendorProps)
        {
            var containerReader = new ContainerItemReader(parseResourceProps, parseVendorProps);
            var mediaItemReader = new MediaItemReader(parseResourceProps, parseVendorProps);

            using var r = XmlReader.Create(new StringReader(content), Settings);

            if(!r.ReadToFollowing("DIDL-Lite", Ns)) yield break;

            var depth = r.Depth;

            while(r.Read() && r.Depth > depth)
            {
                if(r.NodeType != Element || r.NamespaceURI != Ns) continue;

                switch(r.Name)
                {
                    case "container":
                        yield return containerReader.Read(r);
                        break;
                    case "item":
                        yield return mediaItemReader.Read(r);
                        break;
                }
            }
        }
    }
}