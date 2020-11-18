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

            while(r.Read() && r.Depth == 0)
            {
                if(r.NodeType != Element || r.Name != "DIDL-Lite" || r.NamespaceURI != Ns) continue;
                while(r.Read() && r.Depth == 1)
                {
                    if(r.NodeType != Element || r.NamespaceURI != Ns) continue;
                    var item = ReadItem(r, containerReader, mediaItemReader);
                    if(item != null) yield return item;
                }
            }
        }

        public static IEnumerable<Item> ParseLoose(string content)
        {
            var containerReader = new ContainerItemReader(true, true);
            var mediaItemReader = new MediaItemReader(true, true);

            using var r = XmlReader.Create(new StringReader(content), Settings);

            var items = new List<Item>();

            try
            {
                while(r.Read())
                {
                    if(r.NodeType != Element || r.Name != "DIDL-Lite" || r.NamespaceURI != Ns) continue;
                    var depth = r.Depth + 1;
                    while(r.Read() && r.Depth == depth)
                    {
                        if(r.NodeType != Element || r.NamespaceURI != Ns) continue;
                        var item = ReadItem(r, containerReader, mediaItemReader);
                        if(item != null) items.Add(item);
                    }
                }
            }
            catch(XmlException) {}

            return items;
        }

        private static Item ReadItem(XmlReader reader, ContainerItemReader containerReader, MediaItemReader mediaItemReader)
        {
            if(reader.NamespaceURI == Ns)
            {
                switch(reader.Name)
                {
                    case "container":
                        return containerReader.Read(reader);
                    case "item":
                        return mediaItemReader.Read(reader);
                }
            }

            reader.Skip();

            return null;
        }
    }
}