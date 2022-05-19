using static System.StringComparison;

namespace IoT.Protocol.Upnp;

public class SsdpReplyComparer : IEqualityComparer<SsdpReply>
{
    private readonly string key;

    public SsdpReplyComparer(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        this.key = key;
    }

    #region Implementation of IEqualityComparer<in SsdpReply>

    public bool Equals(SsdpReply x, SsdpReply y) =>
        ReferenceEquals(x, y) || x != null && y != null &&
        x.TryGetValue(key, out var v1) &&
        y.TryGetValue(key, out var v2) &&
        string.Equals(v1, v2, OrdinalIgnoreCase);

    public int GetHashCode(SsdpReply obj) => obj.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value) ? value.GetHashCode(InvariantCulture) : 0;

    #endregion
}