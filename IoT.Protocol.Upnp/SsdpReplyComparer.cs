using static System.StringComparison;

namespace IoT.Protocol.Upnp;

#nullable enable

public class SsdpReplyComparer : IEqualityComparer<SsdpReply>
{
    private readonly string key;

    public SsdpReplyComparer(string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        this.key = key;
    }

    #region Implementation of IEqualityComparer<in SsdpReply>

    /// <inheritdoc />
    public bool Equals(SsdpReply? x, SsdpReply? y) =>
        ReferenceEquals(x, y) || x is not null && y is not null &&
        x.TryGetValue(key, out var value1) &&
        y.TryGetValue(key, out var value2) &&
        string.Equals(value1, value2, OrdinalIgnoreCase);

    /// <inheritdoc />
    public int GetHashCode(SsdpReply obj) =>
        obj.TryGetValue(key, out var value) && value is { Length: > 0 } ? value.GetHashCode(OrdinalIgnoreCase) : 0;

    #endregion
}