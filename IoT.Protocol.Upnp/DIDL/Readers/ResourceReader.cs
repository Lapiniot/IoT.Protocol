using System.Collections.Generic;
using System.Xml;
using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Upnp.DIDL.Readers
{
    public class ResourceReader : ReaderBase<Resource>
    {
        private static ResourceReader instance;

        public static ResourceReader Instance => instance ?? (instance = new ResourceReader());

        #region Overrides of ReaderBase<Resource>

        protected override bool TryReadChildNode(XmlReader reader, Resource element)
        {
            if(reader.NodeType == CDATA || reader.NodeType == Text)
            {
                element.Url = reader.ReadContentAsString();
                return true;
            }

            return false;
        }

        protected override Resource CreateElement(XmlReader reader)
        {
            var resource = new Resource();

            if(reader.AttributeCount > 0)
            {
                for(var i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);

                    if(reader.Name == "protocolInfo")
                    {
                        resource.Protocol = reader.Value;
                    }
                    else
                    {
                        resource.Attributes ??= new Dictionary<string, string>();
                        resource.Attributes[reader.Name] = reader.Value;
                    }
                }
            }

            reader.MoveToElement();

            return resource;
        }

        #endregion
    }
}