using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace IoT.Protocol.Soap
{
    internal class SoapHttpContent : HttpContent
    {
        private readonly SoapEnvelope soapEnvelope;
        private readonly Encoding encoding;

        public SoapHttpContent(SoapEnvelope soapEnvelope, Encoding encoding = null)
        {
            this.soapEnvelope = soapEnvelope;
            this.encoding = encoding ?? new UTF8Encoding(false);

            Headers.Add("Content-Type", $"text/xml; charset=\"{this.encoding.WebName}\"");
        }

        #region Overrides of HttpContent

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            return soapEnvelope.WriteAsync(stream, encoding);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        #endregion
    }
}