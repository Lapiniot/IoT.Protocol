using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace IoT.Protocol.Upnp.DIDL.Readers;

public class ContainerItemReader : ItemReader<Container>
{
    public ContainerItemReader(bool parseResourceProps, bool parseVendorProps) : base(parseResourceProps, parseVendorProps) { }

    protected override Container CreateElement(string id, string parentId, bool restricted) => new(id, parentId, restricted);

    protected override Container CreateElement([NotNull] XmlReader reader)
    {
        var container = base.CreateElement(reader);

        container.Searchable = ParseBoolean(reader.GetAttribute("searchable"));
        container.ChildCount = ParseInt(reader.GetAttribute("childCount"));
        container.ChildContainerCount = ParseInt(reader.GetAttribute("childContainerCount"));

        return container;
    }
}