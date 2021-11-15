namespace IoT.Protocol.Upnp.Metadata;

[System.Diagnostics.DebuggerDisplay("{Uri} ({Width}x{Height}/{Depth}, {Mime})")]
public record Icon(Uri Uri, string Mime, int Depth, int Width, int Height);

public record Service(string ServiceType, string ServiceId, Uri MetadataUrl, Uri ControlUrl, Uri EventSubscribeUrl);

public record DeviceDescription(string Udn, string DeviceType, string FriendlyName, string SerialNumber, Uri PresentationUrl,
    string Manufacturer, Uri ManufacturerUrl, string ModelName, string ModelDescription, string ModelNumber, Uri ModelUrl,
    IEnumerable<Icon> Icons, IEnumerable<Service> Services)
{
    public Version Version { get; internal set; } = new Version(1, 0);
}