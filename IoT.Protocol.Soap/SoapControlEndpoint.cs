using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Interfaces;
using static System.Net.DecompressionMethods;
using static System.Net.Http.HttpMethod;
using static System.Threading.LazyThreadSafetyMode;

namespace IoT.Protocol.Soap
{
    public class SoapControlEndpoint : IControlEndpoint<SoapEnvelope, SoapEnvelope>, IDisposable
    {
        private readonly Lazy<HttpClient> clientLazy;
        private readonly HttpClient externalClient;

        public SoapControlEndpoint(HttpClient httpClient)
        {
            externalClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            clientLazy = new Lazy<HttpClient>(externalClient);
        }

        public SoapControlEndpoint(Uri baseAddress)
        {
            if(baseAddress == null) throw new ArgumentNullException(nameof(baseAddress));
            clientLazy = new Lazy<HttpClient>(() => CreateClient(baseAddress), ExecutionAndPublication);
        }

        private HttpClient Client => clientLazy.Value;

        public Task<SoapEnvelope> InvokeAsync(SoapEnvelope message, CancellationToken cancellationToken = default)
        {
            return InvokeAsync(null, message, cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected internal async Task<SoapEnvelope> InvokeAsync(Uri actionUri, SoapEnvelope message, CancellationToken cancellationToken = default)
        {
            if(actionUri != null && actionUri.IsAbsoluteUri) throw new ArgumentException("Invalid uri type. Must be valid relative uri.");

            using var request = CreateRequestMessage(actionUri, message);
            using var response = await Client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var charSet = response.Content.Headers.ContentType?.CharSet?.Trim('"');

            var encoding = charSet != null ? Encoding.GetEncoding(charSet) : Encoding.UTF8;

            using var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var readerWithEncoding = new StreamReader(responseStream, encoding);
            var envelope = SoapEnvelope.Deserialize(readerWithEncoding);

            if(envelope.Action != message.Action + "Response")
            {
                throw new InvalidDataException("Invalid SOAP action response");
            }

            return envelope;
        }

        private HttpRequestMessage CreateRequestMessage(Uri actionUri, SoapEnvelope message)
        {
            return new HttpRequestMessage
            {
                RequestUri = actionUri,
                Method = Post,
                Version = new Version(1, 1),
                Headers = {{"SOAPACTION", $"\"{message.Schema}#{message.Action}\""}},
                Content = new SoapHttpContent(message)
            };
        }

        protected HttpClient CreateClient(Uri baseAddress)
        {
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = GZip,
                UseProxy = false,
                Proxy = null,
                UseCookies = false,
                CookieContainer = null,
                AllowAutoRedirect = false
            };

            return new HttpClient(handler, true)
            {
                BaseAddress = baseAddress,
                DefaultRequestHeaders = {{"Accept-Encoding", "gzip"}}
            };
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                if(externalClient == null && clientLazy.IsValueCreated)
                {
                    clientLazy.Value.Dispose();
                }
            }
        }
    }
}