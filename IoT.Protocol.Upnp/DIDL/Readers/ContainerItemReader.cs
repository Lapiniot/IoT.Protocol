using System.Xml;

namespace IoT.Protocol.Upnp.DIDL.Readers
{
    public class ContainerItemReader : ItemReader<Container>
    {
        private static ContainerItemReader instance;

        public static ContainerItemReader Instance
        {
            get { return instance ?? (instance = new ContainerItemReader()); }
        }

        protected override Container CreateElement(string id, string parentId, bool restricted)
        {
            return new Container(id, parentId, restricted);
        }

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