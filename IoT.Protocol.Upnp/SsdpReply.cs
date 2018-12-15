using System;
using System.Collections.Generic;
using static System.Text.Encoding;

namespace IoT.Protocol.Upnp
{
    public class SsdpReply : Dictionary<string, string>
    {
        private const byte Colon = 0x3a;
        private const byte Space = 0x20;

        public SsdpReply() : base(10, StringComparer.OrdinalIgnoreCase) {}

        public string Location => this["LOCATION"];

        public string UniqueServiceName => this["USN"];

        public string Server => this["SERVER"];

        public string SearchTarget => this["ST"];

        public static SsdpReply Parse(Span<byte> buffer)
        {
            int i;
            if((i = buffer.IndexOfEOL()) >= 0 && ASCII.GetString(buffer.Slice(0, i++)) == "HTTP/1.1 200 OK")
            {
                var reply = new SsdpReply();

                for(var r = buffer.Slice(++i); (i = r.IndexOfEOL()) >= 0; r = r.Slice(++i))
                {
                    var line = r.Slice(0, i++);
                    var index = line.IndexOf(Colon);

                    if(index > 0)
                    {
                        var key = ASCII.GetString(line.Slice(0, index));
                        if(++index < line.Length && line[index] == Space) index++;
                        var value = ASCII.GetString(line.Slice(index));
                        reply.Add(key, value);
                    }
                }

                return reply;
            }

            throw new ApplicationException("Not a SSDP success response");
        }
    }
}