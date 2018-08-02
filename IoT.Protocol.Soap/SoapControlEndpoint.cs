using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Interfaces;
using static System.Net.DecompressionMethods;
using static System.Net.Http.HttpMethod;

namespace IoT.Protocol.Soap
{
    public class SoapControlEndpoint : ConnectedObject, IControlEndpoint<SoapEnvelope, SoapEnvelope>
    {
        private readonly Uri baseUri;
        private HttpMessageHandler handler;
        private HttpClient httpClient;

        public SoapControlEndpoint(HttpMessageHandler messageHandler, Uri baseAddress)
        {
            handler = messageHandler;
            baseUri = baseAddress;
        }

        public SoapControlEndpoint(Uri baseAddress)
        {
            baseUri = baseAddress;
        }

        public Task<SoapEnvelope> InvokeAsync(SoapEnvelope message, CancellationToken cancellationToken = default)
        {
            return InvokeAsync(null, message, cancellationToken);
        }

        protected internal async Task<SoapEnvelope> InvokeAsync(Uri actionUri, SoapEnvelope message, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            CheckConnected();
            if(actionUri != null && actionUri.IsAbsoluteUri) throw new ArgumentException("Invalid uri type. Must be valid relative uri.");

            using(var request = CreateRequestMessage(actionUri, message))
            {
                using(var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();

                    var charSet = response.Content.Headers.ContentType?.CharSet?.Trim('"');

                    var encoding = charSet != null ? Encoding.GetEncoding(charSet) : Encoding.UTF8;

                    using(var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using(var readerWithEncoding = new StreamReader(responseStream, encoding))
                    {
                        var envelope = SoapEnvelope.Deserialize(readerWithEncoding);

                        if(envelope.Action != message.Action + "Response")
                        {
                            throw new InvalidDataException("Invalid SOAP action response");
                        }

                        return envelope;
                    }
                }
            }
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

        protected override void OnConnect()
        {
            var ownsHandler = false;

            if(handler == null)
            {
                handler = new SocketsHttpHandler
                {
                    AutomaticDecompression = GZip,
                    UseProxy = false,
                    Proxy = null,
                    UseCookies = false,
                    CookieContainer = null,
                    AllowAutoRedirect = false
                };

                ownsHandler = true;
            }

            httpClient = new HttpClient(handler, ownsHandler)
            {
                BaseAddress = baseUri,
                DefaultRequestHeaders = {{"Accept-Encoding", "gzip"}}
            };
        }

        protected override void OnClose()
        {
            httpClient.Dispose();
            httpClient = null;
        }
    }
}