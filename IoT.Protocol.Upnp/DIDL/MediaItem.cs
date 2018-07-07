using System;
using System.Collections.Generic;

namespace IoT.Protocol.Upnp.DIDL
{
    public class MediaItem : Item
    {
        public MediaItem(string id, string parentId, bool restricted) : base(id, parentId, restricted)
        {
        }

        public string Description { get; set; }
        public string Creator { get; set; }
        public List<string> Artists { get; set; }
        public List<string> Actors { get; set; }
        public List<string> Authors { get; set; }
        public List<string> Producers { get; set; }
        public List<string> Publishers { get; set; }
        public List<string> Genres { get; set; }
        public string Album { get; set; }
        public string Genre { get; set; }
        public DateTime? Date { get; set; }
        public int? TrackNumber { get; set; }
        public string DiscographyUri { get; set; }
        public string LyricsUri { get; set; }
    }
}