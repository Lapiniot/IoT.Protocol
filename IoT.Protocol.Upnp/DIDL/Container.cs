using System;
using System.Collections.Generic;

namespace IoT.Protocol.Upnp.DIDL
{
    public class Container : Item
    {
        public Container(string id, string parentId, bool restricted) : base(id, parentId, restricted)
        {
        }

        public bool Searchable { get; set; }
        public int? ChildCount { get; set; }
        public int? ChildContainerCount { get; set; }
    }
}