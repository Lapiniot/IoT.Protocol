using System.Diagnostics.CodeAnalysis;
using System.Xml;
using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Upnp.DIDL.Readers;

public abstract class ItemReader<TElementType> : ReaderBase<TElementType> where TElementType : Item
{
    private readonly bool parseResourceProps;
    private readonly bool parseVendorProps;

    protected ItemReader(bool parseResourceProps, bool parseVendorProps)
    {
        this.parseResourceProps = parseResourceProps;
        this.parseVendorProps = parseVendorProps;
    }

    protected override TElementType CreateElement([NotNull] XmlReader reader) =>
        CreateElement(reader.GetAttribute("id"), reader.GetAttribute("parentID"), ParseBoolean(reader.GetAttribute("restricted")));

    protected override bool TryReadChildNode([NotNull] XmlReader reader, [NotNull] TElementType element)
    {
        if (reader.NodeType != Element) return false;

        switch (reader.NamespaceURI)
        {
            case DC:
                switch (reader.LocalName)
                {
                    case "title":
                        element.Title = reader.ReadElementContentAsString();
                        return true;
                }

                break;
            case UPNP:
                switch (reader.LocalName)
                {
                    case "class":
                        element.Class = reader.ReadElementContentAsString();
                        return true;
                    case "albumArtURI":
                        (element.AlbumArts ??= new List<string>()).Add(reader.ReadElementContentAsString());
                        return true;
                    case "storageUsed":
                        element.StorageUsed = reader.ReadElementContentAsInt();
                        return true;
                    case "storageTotal":
                        element.StorageTotal = reader.ReadElementContentAsInt();
                        return true;
                    case "storageFree":
                        element.StorageFree = reader.ReadElementContentAsInt();
                        return true;
                    case "storageMedium":
                        element.StorageMedium = reader.ReadElementContentAsInt();
                        return true;
                }

                break;
            default:
                if (reader.Name == "res")
                {
                    if (parseResourceProps)
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
                    if (parseVendorProps)
                    {
                        (element.Vendor ??= [])[reader.Name] = reader.ReadElementContentAsString();
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