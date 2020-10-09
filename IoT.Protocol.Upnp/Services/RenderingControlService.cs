using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Soap;
using static IoT.Protocol.Upnp.UpnpServices;

namespace IoT.Protocol.Upnp.Services
{
    [ServiceSchema(RenderingControl)]
    public sealed class RenderingControlService : SoapActionInvoker
    {
        public RenderingControlService(SoapControlEndpoint endpoint, Uri controlUri) :
            base(endpoint, controlUri, RenderingControl) {}

        public RenderingControlService(SoapControlEndpoint endpoint) :
            base(endpoint, RenderingControl) {}

        public Task<IDictionary<string, string>> GetVolumeAsync(uint instanceId, CancellationToken cancellationToken = default)
        {
            return InvokeAsync("GetVolume", cancellationToken, ("InstanceID", instanceId), ("Channel", "Master"));
        }

        public Task<IDictionary<string, string>> SetVolumeAsync(uint instanceId, uint volume, CancellationToken cancellationToken = default)
        {
            return InvokeAsync("SetVolume", cancellationToken, ("InstanceID", instanceId), ("Channel", "Master"), ("DesiredVolume", volume));
        }

        public Task<IDictionary<string, string>> GetMuteAsync(uint instanceId, CancellationToken cancellationToken = default)
        {
            return InvokeAsync("GetMute", cancellationToken, ("InstanceID", instanceId), ("Channel", "Master"));
        }

        public Task<IDictionary<string, string>> SetMuteAsync(uint instanceId, bool mute, CancellationToken cancellationToken = default)
        {
            return InvokeAsync("SetMute", cancellationToken, ("InstanceID", instanceId), ("Channel", "Master"), ("DesiredMute", mute));
        }

        public Task<IDictionary<string, string>> GetPresetsAsync(uint instanceId, CancellationToken cancellationToken = default)
        {
            return InvokeAsync("ListPresets", cancellationToken, ("InstanceID", instanceId));
        }
    }
}