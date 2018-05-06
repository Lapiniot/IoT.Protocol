using System;
using System.Collections.Generic;

namespace IoT.Protocol.Upnp
{
    public class SsdpReply : Dictionary<string, string>
    {
        public SsdpReply() : base(10, StringComparer.OrdinalIgnoreCase)
        {
        }

        public string Location { get { return this["LOCATION"]; } }
        public string UniqueServiceName { get { return this["USN"]; } }
        public string Server { get { return this["SERVER"]; } }
        public string SearchTarget { get { return this["ST"]; } }
    }
}