namespace IoT.Protocol.Upnp.DIDL;

public abstract class Item(string id, string parentId, bool restricted)
{
    public string Id { get; set; } = id;
    public string ParentId { get; set; } = parentId;
    public bool Restricted { get; set; } = restricted;
    public string Title { get; set; }
    public string Class { get; set; }
    public int? StorageUsed { get; set; }
    public int? StorageTotal { get; set; }
    public int? StorageFree { get; set; }
    public int? StorageMedium { get; set; }
    public Resource Resource { get; set; }
    public Dictionary<string, string> Vendor { get; internal set; }
    public ICollection<string> AlbumArts { get; internal set; }
}