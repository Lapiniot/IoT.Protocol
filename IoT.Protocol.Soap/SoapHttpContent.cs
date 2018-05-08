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
        private readonly MemoryStream memStream;

        public SoapHttpContent(SoapEnvelope soapEnvelope, Encoding encoding = null)
        {
            memStream = new MemoryStream();
            soapEnvelope.Serialize(memStream, encoding);

            Headers.Add("Content-Type", $"text/xml; charset=\"{(encoding ?? UTF8).WebName}\"");
        }

        #region Overrides of HttpContent

        protected override void Dispose(bool disposing)
        {
            memStream.Dispose();

            base.Dispose(disposing);
        }

        #endregion

        #region Overrides of HttpContent

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            memStream.Seek(0, SeekOrigin.Begin);

            return memStream.CopyToAsync(stream);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = memStream.Length;

            return true;
        }

        #endregion
    }
}