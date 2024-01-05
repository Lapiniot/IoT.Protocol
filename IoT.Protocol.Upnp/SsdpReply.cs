using static System.Text.Encoding;

namespace IoT.Protocol.Upnp;

public class SsdpReply(string startLine) : Dictionary<string, string>(10, StringComparer.OrdinalIgnoreCase)
{
    public string Location => this["LOCATION"];

    public string UniqueServiceName => this["USN"];

    public string Server => this["SERVER"];

    public string SearchTarget => this["ST"];

    public string StartLine { get; } = startLine;

    public double MaxAge => TryGetValue("CACHE-CONTROL", out var value) && value is { Length: > 8 } && int.TryParse(value[8..], out var age) ? age : 0;

    public string BootId => TryGetValue("BOOTID.UPNP.ORG", out var value) ? value : null;

    public string ConfigId => TryGetValue("CONFIGID.UPNP.ORG", out var value) ? value : null;

#pragma warning disable IDE0057 // Use range operator
    public static bool TryParse(ReadOnlySpan<byte> span, out SsdpReply reply)
    {
        int index;

        ReadOnlySpan<byte> EOL = [(byte)'\r', (byte)'\n'];
        if ((index = span.IndexOf(EOL)) < 0)
        {
            reply = null;
            return false;
        }

        reply = new SsdpReply(ASCII.GetString(span.Slice(0, index)));
        span = span.Slice(index + 2);

        while ((index = span.IndexOf(EOL)) > 0)
        {
            var line = span.Slice(0, index);
            span = span.Slice(index + 2);

            var i = line.IndexOf((byte)':');
            if (i <= 0) continue;
            var key = ASCII.GetString(line.Slice(0, i++));
            while (i < line.Length && line[i] == ' ') i++;
            var value = ASCII.GetString(line.Slice(i));
            reply.Add(key, value);
        }

        return true;
    }
#pragma warning restore IDE0057 // Use range operator
}