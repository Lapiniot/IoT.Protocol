using System.Xml.Linq;
using static System.Globalization.CultureInfo;
using static System.String;
using static System.UriPartial;

namespace IoT.Protocol.Upnp;

public class UpnpDeviceDescription
{
    private const string MissingElementFormat = "Missing mandatory \'{0}\' xml element";
    public static readonly XNamespace NS = "urn:schemas-upnp-org:device-1-0";

    public UpnpDeviceDescription(Uri location, IEnumerable<UpnpServiceDescription> services, IEnumerable<Icon> icons, string udn, string deviceType,
        string friendlyName, string manufacturer, string modelDescription, string modelName, string modelNumber,
        Uri manufacturerUri, Uri modelUri, Uri presentationUri)
    {
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(services);

        if(IsNullOrWhiteSpace(udn)) throw new ArgumentException("Shouldn't be null or empty", nameof(udn));
        if(IsNullOrWhiteSpace(deviceType)) throw new ArgumentException("Shouldn't be null or empty", nameof(deviceType));

        Location = location;
        Services = services;
        Icons = icons;
        Udn = udn;
        DeviceType = deviceType;
        FriendlyName = friendlyName;
        Manufacturer = manufacturer;
        ModelDescription = modelDescription;
        ModelName = modelName;
        ModelNumber = modelNumber;
        ManufacturerUri = manufacturerUri;
        ModelUri = modelUri;
        PresentationUri = presentationUri;
    }

    public Uri Location { get; }
    public IEnumerable<UpnpServiceDescription> Services { get; }
    public IEnumerable<Icon> Icons { get; }
    public string Udn { get; }
    public string DeviceType { get; }
    public string FriendlyName { get; }
    public string Manufacturer { get; }
    public string ModelDescription { get; }
    public string ModelName { get; }
    public string ModelNumber { get; }
    public Uri ManufacturerUri { get; }
    public Uri ModelUri { get; }
    public Uri PresentationUri { get; }

    public static UpnpDeviceDescription ParseXml(Stream stream, Uri location)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(location);

        var doc = XDocument.Load(stream);

        var dev = (doc.Root ?? throw new InvalidDataException("Invalid XML document"))
            .Element(NS + "device") ?? throw new InvalidDataException(Format(InvariantCulture, MissingElementFormat, "device"));

        var baseUri = new Uri((doc.Root.Element(NS + "URLBase")?.Value) ?? location.GetLeftPart(Authority));

        var servicesElement = dev.Element(NS + "serviceList") ?? throw new InvalidDataException(Format(InvariantCulture, MissingElementFormat, "serviceList"));
        var services = servicesElement.Elements(NS + "service").Select(s => new UpnpServiceDescription(
                (s.Element(NS + "serviceType") ?? throw new InvalidDataException(Format(InvariantCulture, MissingElementFormat, "serviceType"))).Value,
                (s.Element(NS + "serviceId") ?? throw new InvalidDataException(Format(InvariantCulture, MissingElementFormat, "serviceId"))).Value,
                new Uri(baseUri, (s.Element(NS + "SCPDURL") ?? throw new InvalidDataException(Format(InvariantCulture, MissingElementFormat, "SCPDURL"))).Value),
                new Uri(baseUri, (s.Element(NS + "controlURL") ?? throw new InvalidDataException(Format(InvariantCulture, MissingElementFormat, "controlURL"))).Value),
                new Uri(baseUri, (s.Element(NS + "eventSubURL") ?? throw new InvalidDataException(Format(InvariantCulture, MissingElementFormat, "eventSubURL"))).Value)))
            .ToArray();

        var icons = dev.Element(NS + "iconList")?.Elements(NS + "icon").Select(i => new Icon(
                new Uri(baseUri, (i.Element(NS + "url") ?? throw new InvalidDataException(Format(InvariantCulture, MissingElementFormat, "url"))).Value),
                i.Element(NS + "mimetype")?.Value,
                int.Parse((i.Element(NS + "depth") ?? throw new InvalidDataException(Format(InvariantCulture, MissingElementFormat, "depth"))).Value, InvariantCulture),
                int.Parse((i.Element(NS + "width") ?? throw new InvalidDataException(Format(InvariantCulture, MissingElementFormat, "width"))).Value, InvariantCulture),
                int.Parse((i.Element(NS + "height") ?? throw new InvalidDataException(Format(InvariantCulture, MissingElementFormat, "height"))).Value, InvariantCulture)))
            .ToArray();

        return new UpnpDeviceDescription(location, services, icons ?? Array.Empty<Icon>(),
            (dev.Element(NS + "UDN") ?? throw new InvalidDataException(Format(InvariantCulture, MissingElementFormat, "UDN"))).Value,
            (dev.Element(NS + "deviceType") ?? throw new InvalidDataException(Format(InvariantCulture, MissingElementFormat, "deviceType"))).Value,
            dev.Element(NS + "friendlyName")?.Value,
            dev.Element(NS + "manufacturer")?.Value,
            dev.Element(NS + "modelDescription")?.Value,
            dev.Element(NS + "modelName")?.Value,
            dev.Element(NS + "modelNumber")?.Value,
            BuildUri(dev.Element(NS + "manufacturerURL")?.Value),
            BuildUri(dev.Element(NS + "modelURL")?.Value),
            BuildUri(dev.Element(NS + "presentationURL")?.Value));

        Uri BuildUri(string value)
        {
            return !IsNullOrWhiteSpace(value) && Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out var uri)
                ? uri.IsAbsoluteUri ? uri : new Uri(baseUri, uri)
                : null;
        }
    }
}