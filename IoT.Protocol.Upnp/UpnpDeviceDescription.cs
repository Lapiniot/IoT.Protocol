using System;
using System.Diagnostics.CodeAnalysis;
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
        private static readonly XNamespace NS = "urn:schemas-upnp-org:device-1-0";

        internal UpnpDeviceDescription(Uri baseUri, UpnpServiceDescription[] services, string udn, string deviceType,
            string friendlyName, string manufacturer, string modelDescription, string modelName, string modelNumber)
        {
            if(string.IsNullOrWhiteSpace(udn)) throw new ArgumentException(nameof(udn));
            if(string.IsNullOrWhiteSpace(deviceType)) throw new ArgumentException(nameof(deviceType));

            BaseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
            Services = services ?? throw new ArgumentNullException(nameof(services));
            Udn = udn;
            DeviceType = deviceType;
            FriendlyName = friendlyName;
            Manufacturer = manufacturer;
            ModelDescription = modelDescription;
            ModelName = modelName;
            ModelNumber = modelNumber;
        }

        public Uri BaseUri { get; }
        public UpnpServiceDescription[] Services { get; }
        public string Udn { get; }
        public string DeviceType { get; }
        public string FriendlyName { get; }
        public string Manufacturer { get; }
        public string ModelDescription { get; }
        public string ModelName { get; }
        public string ModelNumber { get; }

        public static async Task<UpnpDeviceDescription> LoadAsync(Uri location, CancellationToken cancellationToken)
        {
            using(var client = new HttpClient())
            using(var response = await client.GetAsync(location, cancellationToken))
            using(var stream = await response.Content.ReadAsStreamAsync())
            {
                var xdoc = XDocument.Load(stream);

                var baseUri = new Uri(location.GetLeftPart(Authority));

                var dev = xdoc.Root.Element(NS + "device");

                var services = dev.Element(NS + "serviceList").Elements(NS + "service").Select(s => new UpnpServiceDescription(
                    s.Element(NS + "serviceType").Value,
                    s.Element(NS + "serviceId").Value,
                    new Uri(baseUri, s.Element(NS + "SCPDURL").Value),
                    new Uri(baseUri, s.Element(NS + "controlURL").Value),
                    new Uri(baseUri, s.Element(NS + "eventSubURL").Value)
                )).ToArray();

                return new UpnpDeviceDescription(baseUri, services,
                    dev.Element(NS + "UDN")?.Value,
                    dev.Element(NS + "deviceType").Value,
                    dev.Element(NS + "friendlyName").Value,
                    dev.Element(NS + "manufacturer").Value,
                    dev.Element(NS + "modelDescription")?.Value,
                    dev.Element(NS + "modelName").Value,
                    dev.Element(NS + "modelNumber")?.Value);
            }
        }
    }
}