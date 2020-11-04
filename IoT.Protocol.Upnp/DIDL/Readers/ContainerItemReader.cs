using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace IoT.Protocol.Upnp.DIDL.Readers
{
    public class ContainerItemReader : ItemReader<Container>
    {
        public ContainerItemReader(bool parseResourceProps, bool parseVendorProps) : base(parseResourceProps, parseVendorProps)
        {
        }

        protected override Container CreateElement(string id, string parentId, bool restricted)
        {
            return new Container(id, parentId, restricted);
        }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
        protected override Container CreateElement(XmlReader reader)
        {
            var container = base.CreateElement(reader);

            container.Searchable = ParseBoolean(reader.GetAttribute("searchable"));
            container.ChildCount = ParseInt(reader.GetAttribute("childCount"));
            container.ChildContainerCount = ParseInt(reader.GetAttribute("childContainerCount"));

            return container;
        }
    }
}