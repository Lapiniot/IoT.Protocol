using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Upnp.DIDL.Readers
{
    [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
    public abstract class ItemReader<TElementType> : ReaderBase<TElementType> where TElementType : Item
    {
        private readonly bool parseResourceProps;
        private readonly bool parseVendorProps;

        protected ItemReader(bool parseResourceProps, bool parseVendorProps)
        {
            this.parseResourceProps = parseResourceProps;
            this.parseVendorProps = parseVendorProps;
        }

        protected override TElementType CreateElement(XmlReader reader)
        {
            return CreateElement(reader.GetAttribute("id"), reader.GetAttribute("parentID"), ParseBoolean(reader.GetAttribute("restricted")));
        }

        protected override bool TryReadChildNode(XmlReader reader, TElementType element)
        {
            if(reader.NodeType != Element) return false;

            switch(reader.NamespaceURI)
            {
                case DC:
                    switch(reader.LocalName)
                    {
                        case "title":
                            element.Title = reader.ReadElementContentAsString();
                            return true;
                    }

                    break;
                case UPNP:
                    switch(reader.LocalName)
                    {
                        case "class":
                            element.Class = reader.ReadElementContentAsString();
                            return true;
                        case "storageUsed":
                            element.StorageUsed = reader.ReadElementContentAsInt();
                            return true;
                        case "albumArtURI":
                            (element.AlbumArts ??= new List<string>()).Add(reader.ReadElementContentAsString());
                            return true;
                    }

                    break;
                default:
                    if(reader.Name == "res")
                    {
                        if(parseResourceProps)
                        {
                            element.Resource = ResourceReader.Instance.Read(reader);
                        }
                        else
                        {
                            reader.Skip();
                        }
                    }
                    else
                    {
                        if(parseVendorProps)
                        {
                            (element.Vendor ??= new Dictionary<string, string>())[reader.Name] = reader.ReadElementContentAsString();
                        }
                        else
                        {
                            reader.Skip();
                        }
                    }

                    return true;
            }

            return false;
        }

        protected abstract TElementType CreateElement(string id, string parentId, bool restricted);
    }
}