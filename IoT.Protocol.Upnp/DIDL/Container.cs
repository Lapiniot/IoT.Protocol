namespace IoT.Protocol.Upnp.DIDL;

public class Container(string id, string parentId, bool restricted) : Item(id, parentId, restricted)
{
    public bool Searchable { get; set; }
    public int? ChildCount { get; set; }
    public int? ChildContainerCount { get; set; }
}