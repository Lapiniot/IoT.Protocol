namespace IoT.Protocol.Upnp.DIDL;

public class MediaItem(string id, string parentId, bool restricted) : Item(id, parentId, restricted)
{
    public string Album { get; set; }
    public string Creator { get; set; }
    public string Description { get; set; }
    public string Genre { get; set; }
    public DateTime? Date { get; set; }
    public int? TrackNumber { get; set; }
    public string DiscographyUrl { get; internal set; }
    public string LyricsUrl { get; internal set; }
    public ICollection<string> Artists { get; internal set; }
    public ICollection<string> Actors { get; internal set; }
    public ICollection<string> Authors { get; internal set; }
    public ICollection<string> Producers { get; internal set; }
    public ICollection<string> Directors { get; internal set; }
    public ICollection<string> Publishers { get; internal set; }
    public ICollection<string> Genres { get; internal set; }
}