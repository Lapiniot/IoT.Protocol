using System;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Upnp.Metadata;

namespace IoT.Protocol.Upnp
{
    public class UpnpServiceDescription
    {
        internal UpnpServiceDescription(string serviceType, string serviceId, Uri metadataUri, Uri controlUri, Uri eventSubscribeUri)
        {
            ServiceType = serviceType;
            ServiceId = serviceId;
            MetadataUri = metadataUri;
            ControlUri = controlUri;
            EventSubscribeUri = eventSubscribeUri;
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

        public dynamic GreateDynamicProxy()
        {
            return new ExpandoObject();
        }

        public UpnpControlEndpoint CreateControlPoint()
        {
            return new UpnpControlEndpoint(ControlUri, ServiceType);
        }
    }
}