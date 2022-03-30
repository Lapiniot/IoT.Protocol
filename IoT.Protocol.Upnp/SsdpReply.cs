using System.Runtime.CompilerServices;
using static System.Text.Encoding;

namespace IoT.Protocol.Upnp;

public class SsdpReply : Dictionary<string, string>
{
    private const byte Colon = 0x3a;
    private const byte Space = 0x20;
    private const byte CR = 0x0d;
    private const byte LF = 0x0a;

    public SsdpReply(string startLine) : base(10, StringComparer.OrdinalIgnoreCase) => StartLine = startLine;

    public string Location => this["LOCATION"];

    public string UniqueServiceName => this["USN"];

    public string Server => this["SERVER"];

    public string SearchTarget => this["ST"];

    public string StartLine { get; }

    public double MaxAge => TryGetValue("CACHE-CONTROL", out var value) && value != null && value.Length > 8 && int.TryParse(value[8..], out var age) ? age : 0;

    public string BootId => TryGetValue("BOOTID.UPNP.ORG", out var value) ? value : null;

    public string ConfigId => TryGetValue("CONFIGID.UPNP.ORG", out var value) ? value : null;

    public static SsdpReply Parse(ReadOnlySpan<byte> span)
    {
        int i;

        if ((i = IndexOfEOL(span)) < 0)
        {
            throw new InvalidDataException("Not a SSDP success response");
        }

        var reply = new SsdpReply(ASCII.GetString(span[..i++]));

        for (var r = span[++i..]; (i = IndexOfEOL(r)) >= 0; r = r[++i..])
        {
            var line = r[..i++];
            var index = line.IndexOf(Colon);

            if (index <= 0) continue;

            var key = ASCII.GetString(line[..index]);
            if (++index < line.Length && line[index] == Space) index++;
            var value = ASCII.GetString(line[index..]);
            reply.Add(key, value);
        }

        return reply;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int IndexOfEOL(ReadOnlySpan<byte> span)
    {
        int index;
        return (index = span.IndexOf(CR)) > 0 && index < span.Length - 1 && span[index + 1] == LF ? index : -1;
    }
}