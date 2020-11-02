using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml;
using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Upnp.DIDL.Readers
{
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
    public class ResourceReader : ReaderBase<Resource>
    {
        private static ResourceReader instance;

        public static ResourceReader Instance => instance ??= new ResourceReader();

        #region Overrides of ReaderBase<Resource>

        protected override bool TryReadChildNode(XmlReader reader, Resource element)
        {
            if(reader.NodeType != CDATA && reader.NodeType != Text) return false;
            element.Url = reader.ReadContentAsString();
            return true;
        }

        protected override Resource CreateElement(XmlReader reader)
        {
            var resource = new Resource();

            if(reader.AttributeCount > 0)
            {
                for(var i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);

                    switch(reader.Name)
                    {
                        case "protocolInfo":
                            resource.Protocol = reader.Value;
                            break;
                        case "size":
                            resource.Size = reader.ReadContentAsLong();
                            break;
                        case "duration":
                            var content = reader.ReadContentAsString();
                            if(TimeSpan.TryParse(content, CultureInfo.InvariantCulture, out var value))
                            {
                                resource.Duration = value;
                            }
                            break;
                        default:
                            resource.Attributes ??= new Dictionary<string, string>();
                            resource.Attributes[reader.Name] = reader.Value;
                            break;
                    }
                }
            }

            reader.MoveToElement();

            return resource;
        }

        #endregion
    }
}