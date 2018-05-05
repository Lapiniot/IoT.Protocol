using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using IoT.Protocol.Upnp.Metadata;

namespace IoT.Protocol.Upnp
{
    public class UpnpServiceDescription
    {

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

        public async Task<ServiceMetadata> GetMetadataAsync(CancellationToken cancellationToken = default)
        {
            return await ServiceMetadata.LoadAsync(MetadataUri, cancellationToken);
        }
    }
}