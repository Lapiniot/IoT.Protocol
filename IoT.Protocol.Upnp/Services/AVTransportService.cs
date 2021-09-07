using IoT.Protocol.Soap;
using static System.Globalization.CultureInfo;
using static IoT.Protocol.Upnp.UpnpServices;

namespace IoT.Protocol.Upnp.Services;

[ServiceSchema(AVTransport)]
[CLSCompliant(false)]
public sealed class AVTransportService : SoapActionInvoker
{
    public AVTransportService(SoapControlEndpoint endpoint, Uri controlUri) :
        base(endpoint, controlUri, AVTransport)
    { }

    public AVTransportService(SoapControlEndpoint endpoint) :
        base(endpoint, AVTransport)
    { }

    public Task<IReadOnlyDictionary<string, string>> GetMediaInfoAsync(uint instanceId = 0, CancellationToken cancellationToken = default)
    {
        return InvokeAsync("GetMediaInfo", new Dictionary<string, string>() {
                { "InstanceID", instanceId.ToString(InvariantCulture) } },
            cancellationToken);
    }

    public Task<IReadOnlyDictionary<string, string>> GetPositionInfoAsync(uint instanceId = 0, CancellationToken cancellationToken = default)
    {
        return InvokeAsync("GetPositionInfo", new Dictionary<string, string>() {
                { "InstanceID", instanceId.ToString(InvariantCulture) } },
            cancellationToken);
    }

    public Task<IReadOnlyDictionary<string, string>> GetTransportInfoAsync(uint instanceId = 0, CancellationToken cancellationToken = default)
    {
        return InvokeAsync("GetTransportInfo", new Dictionary<string, string>() {
                { "InstanceID", instanceId.ToString(InvariantCulture) } },
            cancellationToken);
    }

    public Task SetAVTransportUriAsync(uint instanceId = 0, string currentUri = null, string currentUriMetaData = null,
        CancellationToken cancellationToken = default)
    {
        return InvokeAsync("SetAVTransportURI", new Dictionary<string, string>() {
                { "InstanceID", instanceId.ToString(InvariantCulture) },
                { "CurrentURI", currentUri },
                { "CurrentURIMetaData", currentUriMetaData } },
            cancellationToken);
    }

    public Task SetNextAVTransportUriAsync(uint instanceId = 0, string nextUri = null, string nextUriMetaData = null,
        CancellationToken cancellationToken = default)
    {
        return InvokeAsync("SetNextAVTransportURI", new Dictionary<string, string>() {
                { "InstanceID", instanceId.ToString(InvariantCulture) },
                { "NextURI", nextUri },
                { "NextURIMetaData", nextUriMetaData } },
            cancellationToken);
    }

    public Task StopAsync(uint instanceId = 0, CancellationToken cancellationToken = default)
    {
        return InvokeAsync("Stop", new Dictionary<string, string>() {
                { "InstanceID", instanceId.ToString(InvariantCulture) } },
            cancellationToken);
    }

    public Task PlayAsync(uint instanceId = 0, string speed = "1", CancellationToken cancellationToken = default)
    {
        return InvokeAsync("Play", new Dictionary<string, string>() {
                { "InstanceID", instanceId.ToString(InvariantCulture) },
                { "Speed", speed } },
            cancellationToken);
    }

    public Task PauseAsync(uint instanceId = 0, CancellationToken cancellationToken = default)
    {
        return InvokeAsync("Pause", new Dictionary<string, string>() {
                { "InstanceID", instanceId.ToString(InvariantCulture) } },
            cancellationToken);
    }

    public Task NextAsync(uint instanceId = 0, CancellationToken cancellationToken = default)
    {
        return InvokeAsync("Next", new Dictionary<string, string>() {
                { "InstanceID", instanceId.ToString(InvariantCulture) } },
            cancellationToken);
    }

    public Task PreviousAsync(uint instanceId = 0, CancellationToken cancellationToken = default)
    {
        return InvokeAsync("Previous", new Dictionary<string, string>() {
                { "InstanceID", instanceId.ToString(InvariantCulture) } },
            cancellationToken);
    }

    public Task SeekAsync(uint instanceId = 0, string seekMode = "ABS_TIME", string target = null, CancellationToken cancellationToken = default)
    {
        return InvokeAsync("Seek", new Dictionary<string, string>() {
                { "InstanceID", instanceId.ToString(InvariantCulture) },
                { "Unit", seekMode },
                { "Target", target } },
            cancellationToken);
    }

    public Task SetPlayModeAsync(uint instanceId = 0, string newPlayMode = "NORMAL", CancellationToken cancellationToken = default)
    {
        return InvokeAsync("SetPlayMode", new Dictionary<string, string>() {
                { "InstanceID", instanceId.ToString(InvariantCulture) },
                { "NewPlayMode", newPlayMode } },
            cancellationToken);
    }

    public Task<IReadOnlyDictionary<string, string>> GetCurrentTransportActionsAsync(uint instanceId = 0, CancellationToken cancellationToken = default)
    {
        return InvokeAsync("GetCurrentTransportActions", new Dictionary<string, string>() {
                { "InstanceID", instanceId.ToString(InvariantCulture) } },
            cancellationToken);
    }

    public Task<IReadOnlyDictionary<string, string>> GetTransportSettingsAsync(uint instanceId = 0, CancellationToken cancellationToken = default)
    {
        return InvokeAsync("GetTransportSettings", new Dictionary<string, string>() {
                { "InstanceID", instanceId.ToString(InvariantCulture) } },
            cancellationToken);
    }

    public Task<IReadOnlyDictionary<string, string>> GetDeviceCapabilitiesAsync(uint instanceId = 0, CancellationToken cancellationToken = default)
    {
        return InvokeAsync("GetDeviceCapabilities", new Dictionary<string, string>() {
                { "InstanceID", instanceId.ToString(InvariantCulture) } },
            cancellationToken);
    }
}