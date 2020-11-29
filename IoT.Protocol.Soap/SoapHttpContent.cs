using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static System.Text.Encoding;

namespace IoT.Protocol.Soap
{
    internal class SoapHttpContent : HttpContent
    {
        private readonly SoapEnvelope soapEnvelope;

        public SoapHttpContent(SoapEnvelope soapEnvelope, Encoding encoding = null)
        {
            this.soapEnvelope = soapEnvelope ?? throw new System.ArgumentNullException(nameof(soapEnvelope));
            Headers.Add("Content-Type", $"application/xml; charset=\"{(encoding ?? UTF8).WebName}\"");
        }

        #region Overrides of HttpContent

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            soapEnvelope.Serialize(stream);
            return Task.CompletedTask;
        }

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }

        #endregion
    }
}