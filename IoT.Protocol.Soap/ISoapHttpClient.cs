namespace IoT.Protocol.Soap;

public interface ISoapHttpClient
{
    Task<SoapEnvelope> SendAsync(Uri actionUri, SoapEnvelope envelope, CancellationToken cancellationToken);
}