using static System.Text.Encoding;

namespace IoT.Protocol.Upnp;

public class SsdpReply : Dictionary<string, string>
{
    private const byte Colon = 0x3a;
    private const byte Space = 0x20;

    public SsdpReply(string startLine) : base(10, StringComparer.OrdinalIgnoreCase)
    {
        StartLine = startLine;
    }

    public string Location => this["LOCATION"];

    public string UniqueServiceName => this["USN"];

    public string Server => this["SERVER"];

    public string SearchTarget => this["ST"];

    public string StartLine { get; }

    public double MaxAge => TryGetValue("CACHE-CONTROL", out var value) && value != null && value.Length > 8 && int.TryParse(value[8..], out var age) ? age : 0;

    public string BootId => TryGetValue("BOOTID.UPNP.ORG", out var value) ? value : null;

    public string ConfigId => TryGetValue("CONFIGID.UPNP.ORG", out var value) ? value : null;

    public static SsdpReply Parse(Span<byte> buffer)
    {
        int i;

        if((i = buffer.IndexOfEOL()) < 0)
        {
            throw new InvalidDataException("Not a SSDP success response");
        }

        var reply = new SsdpReply(ASCII.GetString(buffer[..i++]));

        for(var r = buffer[++i..]; (i = r.IndexOfEOL()) >= 0; r = r[++i..])
        {
            var line = r[..i++];
            var index = line.IndexOf(Colon);

            if(index <= 0) continue;

            var key = ASCII.GetString(line[..index]);
            if(++index < line.Length && line[index] == Space) index++;
            var value = ASCII.GetString(line[index..]);
            reply.Add(key, value);
        }

        return reply;
    }
}