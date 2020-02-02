using System;
using System.Xml;
using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Upnp.DIDL.Readers
{
    public abstract class ItemReader<TElementType> : ReaderBase<TElementType> where TElementType : Item
    {
        protected override TElementType CreateElement(XmlReader reader)
        {
            if(reader == null) throw new ArgumentNullException(nameof(reader));

            return CreateElement(reader.GetAttribute("id"), reader.GetAttribute("parentID"), ParseBoolean(reader.GetAttribute("restricted")));
        }

        protected override bool TryReadChildNode(XmlReader reader, TElementType element)
        {
            if(reader == null) throw new ArgumentNullException(nameof(reader));
            if(element == null) throw new ArgumentNullException(nameof(element));

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
                            element.AlbumArts.Add(reader.ReadElementContentAsString());
                            return true;
                    }

                    break;
                default:
                    if(reader.Name == "res")
                    {
                        element.Resource = ResourceReader.Instance.Read(reader);
                    }
                    else
                    {
                        element.Vendor[reader.Name] = reader.ReadElementContentAsString();
                    }

                    return true;
            }

            return false;
        }

        protected abstract TElementType CreateElement(string id, string parentId, bool restricted);
    }
}