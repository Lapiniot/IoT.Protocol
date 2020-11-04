using System.Collections.Generic;

namespace IoT.Protocol.Upnp.DIDL
{
    public abstract class Item
    {
        protected Item(string id, string parentId, bool restricted)
        {
            Id = id;
            ParentId = parentId;
            Restricted = restricted;
        }

        public string Id { get; set; }
        public string ParentId { get; set; }
        public bool Restricted { get; set; }
        public string Title { get; set; }
        public string Class { get; set; }
        public int? StorageUsed { get; set; }
        public Resource Resource { get; set; }
        public Dictionary<string, string> Vendor { get; internal set; }
        public List<string> AlbumArts { get; internal set; }
    }
}