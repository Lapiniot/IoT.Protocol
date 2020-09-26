using System;
using System.Collections.Generic;
using System.IO;
using static System.StringComparison;
using static System.Text.Encoding;

namespace IoT.Protocol.Upnp
{
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

        public static SsdpReply Parse(Span<byte> buffer)
        {
            int i;

            if((i = buffer.IndexOfEOL()) < 0)
            {
                throw new InvalidDataException("Not a SSDP success response");
            }

            var reply = new SsdpReply(ASCII.GetString(buffer.Slice(0, i++)));

            for(var r = buffer.Slice(++i); (i = r.IndexOfEOL()) >= 0; r = r.Slice(++i))
            {
                var line = r.Slice(0, i++);
                var index = line.IndexOf(Colon);

                if(index <= 0) continue;

                var key = ASCII.GetString(line.Slice(0, index));
                if(++index < line.Length && line[index] == Space) index++;
                var value = ASCII.GetString(line.Slice(index));
                reply.Add(key, value);
            }

            return reply;
        }
    }
}