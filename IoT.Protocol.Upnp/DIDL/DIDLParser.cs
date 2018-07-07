using System.Collections.Generic;
using System.IO;
using System.Xml;
using IoT.Protocol.Upnp.DIDL.Readers;
using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Upnp.DIDL
{
    public static class DIDLParser
    {
        private const string NS = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/";

        public static IEnumerable<object> Parse(string content)
        {
            var objects = new List<object>();

            using(var tr = new StringReader(content))
            using(var r = XmlReader.Create(tr))
            {
                if(r.MoveToContent() == Element && r.Name == "DIDL-Lite" && r.NamespaceURI == NS)
                {
                    while(r.Read())
                    {
                        if(r.NodeType == Element)
                        {
                            var item = ReadItem(r);

                            if(item != null) objects.Add(item);
                        }
                    }
                }
            }

            return objects;
        }

        private static object ReadItem(XmlReader reader)
        {
            if(reader.NamespaceURI == NS)
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