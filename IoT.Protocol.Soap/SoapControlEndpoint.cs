using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IoT.Device;
using static System.Net.DecompressionMethods;

namespace IoT.Protocol.Soap
{
    public class SoapControlEndpoint : ConnectedObject, IControlEndpoint<SoapEnvelope, SoapEnvelope>
    {
        private readonly Uri baseUri;
        private HttpClient httpClient;

        public SoapControlEndpoint(Uri baseAddress)
        {
            baseUri = baseAddress;
        }

        public async Task<SoapEnvelope> InvokeAsync(SoapEnvelope message, CancellationToken cancellationToken = default)
        {
            CheckDisposed();
            CheckConnected();

            var soapAction = "\"" + message.Schema + "#" + message.Action + "\"";

            using(var stream = new MemoryStream())
            {
                message.Serialize(stream, Encoding.UTF8);
                stream.Seek(0, SeekOrigin.Begin);

                using(var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Version = new Version(1, 1),
                    Headers = {{"SOAPACTION", soapAction}},
                    Content = new StreamContent(stream) {Headers = {{"Content-Length", stream.Length.ToString()}, {"Content-Type", "text/xml; charset=\"utf-8\""}}}
                })
                {
                    using(var response = await httpClient.SendAsync(request, cancellationToken))
                    {
                        response.EnsureSuccessStatusCode();

                        var charSet = response.Content.Headers.ContentType?.CharSet?.Trim('"');

                        var encoding = charSet != null ? Encoding.GetEncoding(charSet) : Encoding.UTF8;

                        using(var responseStream = await response.Content.ReadAsStreamAsync())
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
        }

        protected override void OnConnect()
        {
            httpClient = new HttpClient(new HttpClientHandler {AutomaticDecompression = GZip}, true)
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