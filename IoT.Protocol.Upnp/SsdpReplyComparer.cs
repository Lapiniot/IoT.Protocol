using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
            if(ReferenceEquals(x, y)) return true;
            if(x == null || y == null) return false;
            return x.TryGetValue(key, out var v1) &&
                   y.TryGetValue(key, out var v2) &&
                   string.Equals(v1, v2, OrdinalIgnoreCase);
        }

        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "obj is never null")]
        public int GetHashCode(SsdpReply obj)
        {
            return obj.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value)
                ? value.GetHashCode(InvariantCulture)
                : 0;
        }

        #endregion
    }
}