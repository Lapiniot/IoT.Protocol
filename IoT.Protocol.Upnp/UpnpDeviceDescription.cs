using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using static System.UriPartial;

namespace IoT.Protocol.Upnp
{
    public class UpnpDeviceDescription
    {
        public static readonly XNamespace NS = "urn:schemas-upnp-org:device-1-0";

        public UpnpDeviceDescription(Uri location, IEnumerable<UpnpServiceDescription> services, IEnumerable<Icon> icons, string udn, string deviceType,
            string friendlyName, string manufacturer, string modelDescription, string modelName, string modelNumber,
            Uri manufacturerUri, Uri modelUri, Uri presentationUri)
        {
            if(string.IsNullOrWhiteSpace(udn)) throw new ArgumentException("Shouldn't be null or empty", nameof(udn));
            if(string.IsNullOrWhiteSpace(deviceType)) throw new ArgumentException("Shouldn't be null or empty", nameof(deviceType));

            Location = location ?? throw new ArgumentNullException(nameof(location));
            Services = services ?? throw new ArgumentNullException(nameof(services));
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
            if(stream is null) throw new ArgumentNullException(nameof(stream));
            if(location is null) throw new ArgumentNullException(nameof(location));

            var xdoc = XDocument.Load(stream);

            var dev = xdoc.Root.Element(NS + "device");

            var baseUri = new Uri(location.GetLeftPart(Authority));

            var services = dev.Element(NS + "serviceList").Elements(NS + "service").Select(s => new UpnpServiceDescription(
                    s.Element(NS + "serviceType").Value,
                    s.Element(NS + "serviceId").Value,
                    new Uri(baseUri, s.Element(NS + "SCPDURL").Value),
                    new Uri(baseUri, s.Element(NS + "controlURL").Value),
                    new Uri(baseUri, s.Element(NS + "eventSubURL").Value)))
                .ToArray();

            var icons = dev.Element(NS + "iconList")?.Elements(NS + "icon").Select(i => new Icon(
                    new Uri(baseUri, i.Element(NS + "url").Value),
                    i.Element(NS + "mimetype").Value,
                    int.Parse(i.Element(NS + "depth").Value, CultureInfo.InvariantCulture),
                    int.Parse(i.Element(NS + "width").Value, CultureInfo.InvariantCulture),
                    int.Parse(i.Element(NS + "height").Value, CultureInfo.InvariantCulture)))
                .ToArray();

            return new UpnpDeviceDescription(location, services, icons ?? Array.Empty<Icon>(),
                dev.Element(NS + "UDN").Value,
                dev.Element(NS + "deviceType").Value,
                dev.Element(NS + "friendlyName").Value,
                dev.Element(NS + "manufacturer").Value,
                dev.Element(NS + "modelDescription")?.Value,
                dev.Element(NS + "modelName").Value,
                dev.Element(NS + "modelNumber")?.Value,
                BuildUri(dev.Element(NS + "manufacturerURL")?.Value),
                BuildUri(dev.Element(NS + "modelURL")?.Value),
                BuildUri(dev.Element(NS + "presentationURL")?.Value));

            Uri BuildUri(string value)
            {
                return !string.IsNullOrWhiteSpace(value) && Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out var uri)
                    ? uri.IsAbsoluteUri ? uri : new Uri(baseUri, uri)
                    : null;
            }
        }
    }
}