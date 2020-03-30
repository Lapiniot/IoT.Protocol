using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.UriPartial;

namespace IoT.Protocol.Upnp
{
    public class UpnpDeviceDescription
    {
        public static readonly XNamespace NS = "urn:schemas-upnp-org:device-1-0";

        internal UpnpDeviceDescription(Uri location, IEnumerable<UpnpServiceDescription> services, IEnumerable<Icon> icons, string udn, string deviceType,
            string friendlyName, string manufacturer, string modelDescription, string modelName, string modelNumber)
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

        public static async Task<UpnpDeviceDescription> LoadAsync(Uri location, CancellationToken cancellationToken)
        {
            if(location == null) throw new ArgumentNullException(nameof(location));

            using var client = new HttpClient();
            using var response = await client.GetAsync(location, cancellationToken).ConfigureAwait(false);
            await using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var x = XDocument.Load(stream);

            var baseUri = new Uri(location.GetLeftPart(Authority));

            var dev = x.Root.Element(NS + "device");

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
                dev.Element(NS + "modelNumber")?.Value);
        }
    }
}