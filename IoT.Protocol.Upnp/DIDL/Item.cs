﻿using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace IoT.Protocol.Upnp.DIDL
{
    [JsonConverter(typeof(ItemJsonConverter))]
    public abstract class Item
    {
        protected Item(string id, string parentId, bool restricted)
        {
            Id = id;
            ParentId = parentId;
            Restricted = restricted;
            Vendor = new Dictionary<string, string>();
            AlbumArts = new List<string>();
        }

        public string Id { get; set; }
        public string ParentId { get; set; }
        public bool Restricted { get; set; }
        public string Title { get; set; }
        public Dictionary<string, string> Vendor { get; }
        public string Class { get; set; }
        public int? StorageUsed { get; set; }
        public Resource Resource { get; set; }
        public List<string> AlbumArts { get; }
    }
}