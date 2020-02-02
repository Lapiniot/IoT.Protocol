using System;
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
        private static readonly XNamespace Ns = "urn:schemas-upnp-org:device-1-0";

        internal UpnpDeviceDescription(Uri location, UpnpServiceDescription[] services, Icon[] icons, string udn, string deviceType,
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
        public UpnpServiceDescription[] Services { get; }
        public Icon[] Icons { get; }
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

            var dev = x.Root.Element(Ns + "device");

            var services = dev.Element(Ns + "serviceList").Elements(Ns + "service").Select(s => new UpnpServiceDescription(
                s.Element(Ns + "serviceType").Value,
                s.Element(Ns + "serviceId").Value,
                new Uri(baseUri, s.Element(Ns + "SCPDURL").Value),
                new Uri(baseUri, s.Element(Ns + "controlURL").Value),
                new Uri(baseUri, s.Element(Ns + "eventSubURL").Value)
            )).ToArray();

            var icons = dev.Element(Ns + "iconList")?.Elements(Ns + "icon").Select(i => new Icon(
                new Uri(baseUri, i.Element(Ns + "url").Value),
                i.Element(Ns + "mimetype").Value,
                int.Parse(i.Element(Ns + "depth").Value, CultureInfo.InvariantCulture),
                int.Parse(i.Element(Ns + "width").Value, CultureInfo.InvariantCulture),
                int.Parse(i.Element(Ns + "height").Value, CultureInfo.InvariantCulture)
            )).ToArray();

            return new UpnpDeviceDescription(location, services, icons ?? Array.Empty<Icon>(),
                dev.Element(Ns + "UDN").Value,
                dev.Element(Ns + "deviceType").Value,
                dev.Element(Ns + "friendlyName").Value,
                dev.Element(Ns + "manufacturer").Value,
                dev.Element(Ns + "modelDescription")?.Value,
                dev.Element(Ns + "modelName").Value,
                dev.Element(Ns + "modelNumber")?.Value);
        }
    }
}