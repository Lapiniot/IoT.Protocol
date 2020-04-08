using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Upnp
{
    public static class EventMessageParser
    {
        private const string EventNS = "urn:schemas-upnp-org:event-1-0";
        private const string MetadataNS = "urn:schemas-upnp-org:metadata-1-0/AVT/";

        public static async Task<string> ReadLastChangeContentAsync(XmlReader reader)
        {
            if(reader is null) throw new ArgumentNullException(nameof(reader));

            while(await reader.ReadAsync().ConfigureAwait(false) && reader.Depth == 0)
            {
                if(reader.NodeType != Element || reader.LocalName != "propertyset" || reader.NamespaceURI != EventNS) continue;

                while(await reader.ReadAsync().ConfigureAwait(false) && reader.Depth == 1)
                {
                    if(reader.NodeType != Element || reader.LocalName != "property" || reader.NamespaceURI != EventNS) continue;

                    while(await reader.ReadAsync().ConfigureAwait(false) && reader.Depth == 2)
                    {
                        if(reader.NodeType != Element || reader.Name != "LastChange") continue;
                        return await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                    }
                }
            }

            return null;
        }

        public static async Task<IDictionary<string, string>> ParseAsync(XmlReader reader)
        {
            var content = await ReadLastChangeContentAsync(reader).ConfigureAwait(false);

            if(string.IsNullOrEmpty(content)) return null;

            var map = new Dictionary<string, string>();

            using var innerReader = XmlReader.Create(new StringReader(content),
                new XmlReaderSettings
                {
                    CloseInput = true, ConformanceLevel = ConformanceLevel.Fragment,
                    IgnoreProcessingInstructions = true, IgnoreWhitespace = true, IgnoreComments = true
                });

            while(innerReader.Read() && innerReader.Depth == 0)
            {
                if(innerReader.NodeType != Element || innerReader.LocalName != "Event" || innerReader.NamespaceURI != MetadataNS) continue;

                while(innerReader.Read() && innerReader.Depth == 1)
                {
                    if(innerReader.NodeType != Element && innerReader.LocalName != "InstanceID" || innerReader.NamespaceURI != MetadataNS) continue;

                    while(innerReader.Read() && innerReader.Depth == 2)
                    {
                        if(innerReader.NodeType != Element) continue;
                        var name = innerReader.Name;
                        if(innerReader.MoveToAttribute("val"))
                        {
                            map[name] = innerReader.ReadContentAsString();
                        }
                        else
                        {
                            map[name] = innerReader.ReadElementContentAsString();
                        }
                    }
                }
            }

            return map;
        }
    }
}