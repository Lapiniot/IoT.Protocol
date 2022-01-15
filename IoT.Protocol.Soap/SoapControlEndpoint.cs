using IoT.Protocol.Interfaces;

namespace IoT.Protocol.Soap;

public class SoapControlEndpoint : IControlEndpoint<SoapEnvelope, SoapEnvelope>
{
    private readonly ISoapHttpClient client;

    public SoapControlEndpoint(ISoapHttpClient soapHttpClient)
    {
        ArgumentNullException.ThrowIfNull(soapHttpClient);
        client = soapHttpClient;
    }

    public Task<SoapEnvelope> InvokeAsync(SoapEnvelope command, CancellationToken cancellationToken = default)
    {
        return InvokeAsync(null, command, cancellationToken);
    }

    protected internal Task<SoapEnvelope> InvokeAsync(Uri actionUri, SoapEnvelope message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        return actionUri == null || !actionUri.IsAbsoluteUri
            ? client.SendAsync(actionUri, message, cancellationToken)
            : throw new ArgumentException("Invalid uri type. Must be valid relative uri.");
    }
}