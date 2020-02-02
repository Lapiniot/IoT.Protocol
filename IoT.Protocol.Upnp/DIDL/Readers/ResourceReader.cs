using System;
using System.Collections.Generic;
using System.Xml;
using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Upnp.DIDL.Readers
{
    public class ResourceReader : ReaderBase<Resource>
    {
        private static ResourceReader instance;

        public static ResourceReader Instance => instance ??= new ResourceReader();

        #region Overrides of ReaderBase<Resource>

        protected override bool TryReadChildNode(XmlReader reader, Resource element)
        {
            if(reader == null) throw new ArgumentNullException(nameof(reader));
            if(element == null) throw new ArgumentNullException(nameof(element));

            if(reader.NodeType != CDATA && reader.NodeType != Text) return false;
            element.Url = reader.ReadContentAsString();
            return true;
        }

        protected override Resource CreateElement(XmlReader reader)
        {
            if(reader == null) throw new ArgumentNullException(nameof(reader));

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