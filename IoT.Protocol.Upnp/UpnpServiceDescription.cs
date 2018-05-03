using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IoT.Protocol.Upnp
{
    public class UpnpServiceDescription
    {
        private static XNamespace NS = "urn:schemas-upnp-org:device-1-0";
        internal UpnpServiceDescription(string serviceType, string serviceId, Uri metadataUri, Uri controlUri, Uri eventSubscribeUri)
        {
            this.ServiceType = serviceType;
            this.ServiceId = serviceId;
            this.MetadataUri = metadataUri;
            this.ControlUri = controlUri;
            this.EventSubscribeUri = eventSubscribeUri;
        }

        public string ServiceType { get; }
        public string ServiceId { get; }
        public Uri MetadataUri { get; }
        public Uri ControlUri { get; }
        public Uri EventSubscribeUri { get; }

        public async Task<UpnpServiceMetadata> GetMetadataAsync(CancellationToken cancellationToken)
        {
            using(var client = new HttpClient())
            using(var response = await client.GetAsync(MetadataUri, cancellationToken))
            using(var stream = await response.Content.ReadAsStreamAsync())
            {
                var xdoc = XDocument.Load(stream);

                var actions = xdoc.Root.Element(NS + "actionList").Elements(NS + "action").
                    Select(a=>new UpnpServiceActionMetadata()).ToArray();

                return new UpnpServiceMetadata(actions);
            }
        }
    }
}