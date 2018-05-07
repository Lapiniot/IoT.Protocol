using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.DecompressionMethods;

namespace IoT.Protocol.Soap
{
    public class SoapControlEndpoint : IControlEndpoint<SoapEnvelope, SoapEnvelope>
    {
        private readonly Uri baseUri;
        private readonly object syncRoot = new object();
        private bool connected;
        private HttpClient httpClient;

        public SoapControlEndpoint(Uri baseAddress)
        {
            baseUri = baseAddress;
        }

        public void Close()
        {
            if(connected)
            {
                lock(syncRoot)
                {
                    if(connected)
                    {
                        httpClient.Dispose();
                        httpClient = null;
                        connected = false;
                    }
                }
            }
        }

        public void Connect()
        {
            if(!connected)
            {
                lock(syncRoot)
                {
                    if(!connected)
                    {
                        httpClient = new HttpClient(new HttpClientHandler {AutomaticDecompression = GZip}, true)
                        {
                            BaseAddress = baseUri,
                            DefaultRequestHeaders = {{"Accept-Encoding", "gzip"}}
                        };
                        connected = true;
                    }
                }
            }
        }

        public async Task<SoapEnvelope> InvokeAsync(SoapEnvelope message, CancellationToken cancellationToken = default)
        {
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

        #region IDisposable Support

        private bool disposed; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if(!disposed)
            {
                if(disposing)
                {
                    Close();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}