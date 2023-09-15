using IoT.Protocol.Soap;

namespace IoT.Protocol.Upnp;

public class UpnpControlEndpoint(string serviceType, ISoapHttpClient soapHttpClient) : SoapControlEndpoint(soapHttpClient)
{
    public string ServiceType { get; } = serviceType;

    public async Task<IReadOnlyDictionary<string, string>> InvokeAsync(string action, IReadOnlyDictionary<string, string> args, CancellationToken cancellationToken = default) =>
        (await InvokeAsync(new(action, ServiceType, args), cancellationToken).ConfigureAwait(false)).Arguments;
}