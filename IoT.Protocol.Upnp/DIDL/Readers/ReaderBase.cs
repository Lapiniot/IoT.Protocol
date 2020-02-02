using System;
using System.Xml;
using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Upnp.DIDL.Readers
{
    public abstract class ReaderBase<TElementType> where TElementType : class
    {
        protected const string Ns = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/";
        protected const string DC = "http://purl.org/dc/elements/1.1/";
        protected const string UPNP = "urn:schemas-upnp-org:metadata-1-0/upnp/";

        public TElementType Read(XmlReader reader)
        {
            if(reader == null) throw new ArgumentNullException(nameof(reader));
            var nodeName = reader.Name;
            var depth = reader.Depth;

            var element = CreateElement(reader);

            reader.Read();

            while(!(reader.NodeType == EndElement && reader.Name == nodeName) && reader.Depth > depth)
            {
                if((reader.NodeType == Element || reader.NodeType == Text || reader.NodeType == CDATA) &&
                   TryReadChildNode(reader, element))
                {
                    continue;
                }

                reader.Read();
            }

            return element;
        }

        protected abstract bool TryReadChildNode(XmlReader reader, TElementType element);

        protected abstract TElementType CreateElement(XmlReader reader);

        protected static int? ParseInt(string str)
        {
            return int.TryParse(str, out var value) ? (int?)value : null;
        }

        protected static bool ParseBoolean(string str)
        {
            return str == "1" || bool.TryParse(str, out var value) && value;
        }
    }
}