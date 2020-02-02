using System.Collections.Generic;
using System.IO;
using System.Xml;
using IoT.Protocol.Upnp.DIDL.Readers;
using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Upnp.DIDL
{
    public static class DIDLParser
    {
        private const string Ns = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/";

        public static IEnumerable<Item> Parse(string content)
        {
            var objects = new List<Item>();

            using(var tr = new StringReader(content))
            {
                using var r = XmlReader.Create(tr);
                if(r.MoveToContent() != Element || r.Name != "DIDL-Lite" || r.NamespaceURI != Ns) return objects;
                while(r.Read())
                {
                    if(r.NodeType != Element) continue;

                    var item = ReadItem(r);

                    if(item != null) objects.Add(item);
                }
            }

            return objects;
        }

        private static Item ReadItem(XmlReader reader)
        {
            if(reader.NamespaceURI == Ns)
            {
                switch(reader.Name)
                {
                    case "container":
                        return ContainerItemReader.Instance.Read(reader);
                    case "item":
                        return MediaItemReader.Instance.Read(reader);
                }
            }

            reader.Skip();

            return null;
        }
    }
}