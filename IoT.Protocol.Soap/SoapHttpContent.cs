using System.Net;
using System.Text;

namespace IoT.Protocol.Soap;

internal sealed class SoapHttpContent : HttpContent
{
    private readonly SoapEnvelope soapEnvelope;
    private readonly Encoding encoding;
    private readonly MemoryStream memoryStream;

    public SoapHttpContent(SoapEnvelope soapEnvelope, bool useChankedEncoding, Encoding encoding = null)
    {
        this.soapEnvelope = soapEnvelope;
        this.encoding = encoding ?? new UTF8Encoding(false);

        if (!useChankedEncoding)
        {
            memoryStream = new MemoryStream();
            soapEnvelope.Write(memoryStream, this.encoding);
        }

        Headers.Add("Content-Type", $"text/xml; charset=\"{this.encoding.WebName}\"");
        Headers.Add("SOAPACTION", $"\"{soapEnvelope.Schema}#{soapEnvelope.Action}\"");
    }

    #region Overrides of HttpContent

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
    {
        if (memoryStream is not null)
        {
            _ = memoryStream.Seek(0, SeekOrigin.Begin);
            var vt = stream.WriteAsync(memoryStream.GetBuffer());
            return vt.IsCompletedSuccessfully ? Task.CompletedTask : vt.AsTask();
        }
        else
        {
            return soapEnvelope.WriteAsync(stream, encoding);
        }
    }

    protected override bool TryComputeLength(out long length)
    {
        if (memoryStream is not null)
        {
            length = memoryStream.Length;
            return true;
        }
        else
        {
            length = -1;
            return false;
        }
    }

    protected override void Dispose(bool disposing)
    {
        using (memoryStream)
        {
            base.Dispose(disposing);
        }
    }

    #endregion
}