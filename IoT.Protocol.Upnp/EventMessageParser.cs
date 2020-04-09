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

        public static async Task<(IDictionary<string, string> Metadata, IDictionary<string, string> Vendor)> ParseAsync(XmlReader reader)
        {
            var content = await ReadLastChangeContentAsync(reader).ConfigureAwait(false);

            if(string.IsNullOrEmpty(content)) return default;

            var metadata = new Dictionary<string, string>();
            var vendor = new Dictionary<string, string>();

            using var ir = XmlReader.Create(new StringReader(content),
                new XmlReaderSettings
                {
                    CloseInput = true, ConformanceLevel = ConformanceLevel.Fragment,
                    IgnoreProcessingInstructions = true, IgnoreWhitespace = true, IgnoreComments = true
                });

            while(ir.Read() && ir.Depth == 0)
            {
                if(ir.NodeType != Element || ir.LocalName != "Event" || ir.NamespaceURI != MetadataNS) continue;

                while(ir.Read() && ir.Depth == 1)
                {
                    if(ir.NodeType != Element && ir.LocalName != "InstanceID" || ir.NamespaceURI != MetadataNS) continue;

                    while(ir.Read() && ir.Depth == 2)
                    {
                        if(ir.NodeType != Element) continue;
                        if(ir.NamespaceURI == MetadataNS)
                        {
                            metadata[ir.LocalName] = ir.MoveToAttribute("val") ? ir.ReadContentAsString() : ir.ReadElementContentAsString();
                        }
                        else
                        {
                            vendor[ir.Name] = ir.MoveToAttribute("val") ? ir.ReadContentAsString() : ir.ReadElementContentAsString();
                        }
                    }
                }
            }

            return (metadata, vendor);
        }
    }
}