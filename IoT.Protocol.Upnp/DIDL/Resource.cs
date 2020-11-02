using System;
using System.Collections.Generic;

namespace IoT.Protocol.Upnp.DIDL
{
    public class Resource
    {
        public Dictionary<string, string> Attributes { get; internal set; }

        public string Protocol { get; set; }

        public string Url { get; set; }

        public long? Size { get; set; }

        public TimeSpan? Duration { get; set; }
    }
}