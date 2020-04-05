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
        public static async Task<string> ReadPropertyAsync(XmlReader reader)
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
            var map = new Dictionary<string, string>();
            var content = await ReadPropertyAsync(reader).ConfigureAwait(false);
            using var r = XmlReader.Create(new StringReader(content));
            while(r.Read() && r.Depth == 0)
            {
                if(r.NodeType != Element || r.LocalName != "Event" || r.NamespaceURI != MetadataNS) continue;

                while(r.Read() && r.Depth == 1)
                {
                    if(r.NodeType != Element && r.LocalName != "InstanceID" || r.NamespaceURI != MetadataNS) continue;

                    while(r.Read() && r.Depth == 2)
                    {
                        if(r.NodeType != Element) continue;
                        var name = r.LocalName;
                        if(r.MoveToAttribute("val"))
                        {
                            map[name] = r.ReadContentAsString();
                        }
                        else
                        {
                            map[name] = r.ReadElementContentAsString();
                        }
                    }
                }
            }
            return map;
        }
    }
}