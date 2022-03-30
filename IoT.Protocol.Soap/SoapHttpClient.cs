using System.Net;
using System.Text;

namespace IoT.Protocol.Soap;

public sealed class SoapHttpClient : ISoapHttpClient
{
    private readonly HttpClient client;

    public SoapHttpClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);

        client = httpClient;
        client.DefaultRequestVersion = new Version(1, 1);
        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate");
    }

    public HttpClient Client => client;

    public async Task<SoapEnvelope> SendAsync(Uri actionUri, SoapEnvelope envelope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        using var request = new HttpRequestMessage(HttpMethod.Post, actionUri) { Content = new SoapHttpContent(envelope, false) };
        using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);

        try
        {
            _ = response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException hre) when (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            var error = await ParseResponseAsync(response).ConfigureAwait(false);
            var code = error.Arguments.TryGetValue("errorCode", out var value) && int.TryParse(value, out var c) ? c : 0;
            var description = error.Arguments.TryGetValue("errorDescription", out value) ? value : string.Empty;
            throw new SoapException($"SOAP action invocation error: {error["faultstring"]}", hre, code, description);
        }

        var result = await ParseResponseAsync(response).ConfigureAwait(false);

        return result.Action == envelope.Action + "Response"
            ? result
            : throw new InvalidDataException("Invalid SOAP action response");
    }

    private static async Task<SoapEnvelope> ParseResponseAsync(HttpResponseMessage response)
    {
        var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        await using (responseStream.ConfigureAwait(false))
        {
            var charSet = response.Content.Headers.ContentType?.CharSet?.Trim('"');
            var encoding = charSet != null ? Encoding.GetEncoding(charSet) : Encoding.UTF8;
            using var reader = new StreamReader(responseStream, encoding);
            return SoapEnvelope.Deserialize(reader);
        }
    }
}