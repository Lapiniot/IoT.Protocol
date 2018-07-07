﻿using System.Collections.Generic;

namespace IoT.Protocol.Upnp.DIDL
{
    public class Resource
    {
        public Dictionary<string, string> Attributes { get; set; }

        public string Protocol { get; set; }

        public string Url { get; set; }
    }
}