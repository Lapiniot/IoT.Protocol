using System;
using System.Collections.Generic;
using static System.StringComparison;

namespace IoT.Protocol.Upnp
{
    public class SsdpReplyComparer : IEqualityComparer<SsdpReply>
    {
        private readonly string key;

        public SsdpReplyComparer(string key)
        {
            this.key = key ?? throw new ArgumentNullException(nameof(key));
        }

        #region Implementation of IEqualityComparer<in SsdpReply>

        public bool Equals(SsdpReply x, SsdpReply y)
        {
            return x.TryGetValue(key, out var id1) && y.TryGetValue(key, out var id2) &&
                   string.Equals(id1, id2, OrdinalIgnoreCase);
        }

        public int GetHashCode(SsdpReply obj)
        {
            if(obj == null || !obj.TryGetValue(key, out var id) || string.IsNullOrEmpty(id)) return 0;
            return id.GetHashCode();
        }

        #endregion
    }
}