using IoT.Protocol.Soap;
using static System.Globalization.CultureInfo;
using static IoT.Protocol.Upnp.UpnpServices;

namespace IoT.Protocol.Upnp.Services;

[ServiceSchema(RenderingControl)]
[CLSCompliant(false)]
public sealed class RenderingControlService : SoapActionInvoker
{
    public RenderingControlService(SoapControlEndpoint endpoint, Uri controlUri) :
        base(endpoint, controlUri, RenderingControl)
    { }

    public RenderingControlService(SoapControlEndpoint endpoint) :
        base(endpoint, RenderingControl)
    { }

    public Task<IReadOnlyDictionary<string, string>> GetVolumeAsync(uint instanceId, CancellationToken cancellationToken = default)
    {
        return InvokeAsync("GetVolume", new Dictionary<string, string>() {
                { "InstanceID", instanceId.ToString(InvariantCulture) },
                { "Channel", "Master" } }, cancellationToken);
    }

    public Task<IReadOnlyDictionary<string, string>> SetVolumeAsync(uint instanceId, uint volume, CancellationToken cancellationToken = default)
    {
        return InvokeAsync("SetVolume", new Dictionary<string, string>() {
                { "InstanceID", instanceId.ToString(InvariantCulture) },
                { "Channel", "Master" },
                { "DesiredVolume", volume.ToString(InvariantCulture) } },
            cancellationToken);
    }

    public Task<IReadOnlyDictionary<string, string>> GetMuteAsync(uint instanceId, CancellationToken cancellationToken = default)
    {
        return InvokeAsync("GetMute", new Dictionary<string, string>() {
                { "InstanceID", instanceId.ToString(InvariantCulture) },
                { "Channel", "Master" } },
            cancellationToken);
    }

    public Task<IReadOnlyDictionary<string, string>> SetMuteAsync(uint instanceId, bool mute, CancellationToken cancellationToken = default)
    {
        return InvokeAsync("SetMute", new Dictionary<string, string>() {
                { "InstanceID", instanceId.ToString(InvariantCulture) },
                { "Channel", "Master" },
                { "DesiredMute", mute ? "true" : "false" } },
            cancellationToken);
    }

    public Task<IReadOnlyDictionary<string, string>> GetPresetsAsync(uint instanceId, CancellationToken cancellationToken = default)
    {
        return InvokeAsync("ListPresets", new Dictionary<string, string>() {
                { "InstanceID", instanceId.ToString(InvariantCulture) } },
            cancellationToken);
    }
}