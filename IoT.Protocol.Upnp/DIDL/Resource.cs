namespace IoT.Protocol.Upnp.DIDL;

public class Resource
{
    public string Protocol { get; set; }
    public string Url { get; set; }
    public long? Size { get; set; }
    public TimeSpan? Duration { get; set; }
    public int? BitRate { get; internal set; }
    public int? SampleFrequency { get; internal set; }
    public int? BitsPerSample { get; internal set; }
    public int? NrAudioChannels { get; internal set; }
    public int? ColorDepth { get; internal set; }
    public string Resolution { get; internal set; }
    public string ContentInfoUrl { get; internal set; }
    public string Protection { get; internal set; }
    public Dictionary<string, string> Attributes { get; internal set; }
}