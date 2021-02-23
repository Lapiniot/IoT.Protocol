using System;
using System.Collections.Generic;

namespace IoT.Protocol.Upnp.DIDL
{
    public class MediaItem : Item
    {
        public MediaItem(string id, string parentId, bool restricted) : base(id, parentId, restricted) { }

        public string Album { get; set; }
        public string Creator { get; set; }
        public string Description { get; set; }
        public string Genre { get; set; }
        public DateTime? Date { get; set; }
        public int? TrackNumber { get; set; }
        public string DiscographyUrl { get; internal set; }
        public string LyricsUrl { get; internal set; }
        public List<string> Artists { get; internal set; }
        public List<string> Actors { get; internal set; }
        public List<string> Authors { get; internal set; }
        public List<string> Producers { get; internal set; }
        public List<string> Directors { get; internal set; }
        public List<string> Publishers { get; internal set; }
        public List<string> Genres { get; internal set; }
    }
}