using System.Net;
using System.Text;
using IoT.Protocol.Interfaces;
using static System.Net.Http.HttpMethod;

namespace IoT.Protocol.Soap;

public class SoapControlEndpoint : IControlEndpoint<SoapEnvelope, SoapEnvelope>
{
    private readonly HttpClient client;

    public SoapControlEndpoint(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        client = httpClient;
    }

    public Task<SoapEnvelope> InvokeAsync(SoapEnvelope message, CancellationToken cancellationToken = default)
    {
        return InvokeAsync(null, message, cancellationToken);
    }

    protected internal async Task<SoapEnvelope> InvokeAsync(Uri actionUri, SoapEnvelope message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        if(actionUri != null && actionUri.IsAbsoluteUri) throw new ArgumentException("Invalid uri type. Must be valid relative uri.");

        using var request = new HttpRequestMessage(Post, actionUri) { Content = new SoapHttpContent(message, false) };
        using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

        try
        {
            _ = response.EnsureSuccessStatusCode();
        }
        catch(HttpRequestException hre) when(response.StatusCode == HttpStatusCode.InternalServerError)
        {
            var error = await ParseResponseAsync(response).ConfigureAwait(false);
            var code = error.Arguments.TryGetValue("errorCode", out var value) && int.TryParse(value, out var c) ? c : 0;
            var description = error.Arguments.TryGetValue("errorDescription", out value) ? value : string.Empty;
            throw new SoapException($"SOAP action invocation error: {error["faultstring"]}", hre, code, description);
        }

        var envelope = await ParseResponseAsync(response).ConfigureAwait(false);

        return envelope.Action == message.Action + "Response"
            ? envelope
            : throw new InvalidDataException("Invalid SOAP action response");
    }

    private static async Task<SoapEnvelope> ParseResponseAsync(HttpResponseMessage response)
    {
        var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        await using(responseStream.ConfigureAwait(false))
        {
            var charSet = response.Content.Headers.ContentType?.CharSet?.Trim('"');
            var encoding = charSet != null ? Encoding.GetEncoding(charSet) : Encoding.UTF8;
            using var reader = new StreamReader(responseStream, encoding);
            return SoapEnvelope.Deserialize(reader);
        }
    }
}