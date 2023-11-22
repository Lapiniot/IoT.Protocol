using System.Net;
using System.Text;

namespace IoT.Protocol.Soap;

internal sealed class SoapHttpContent : HttpContent
{
    private readonly Encoding encoding;
    private readonly MemoryStream memoryStream;
    private readonly SoapEnvelope soapEnvelope;

    public SoapHttpContent(SoapEnvelope soapEnvelope, bool useChunkedEncoding, Encoding encoding = null)
    {
        this.soapEnvelope = soapEnvelope;
        this.encoding = encoding ?? new UTF8Encoding(false);

        if (!useChunkedEncoding)
        {
            memoryStream = new();
            soapEnvelope.Write(memoryStream, this.encoding);
        }

        Headers.Add("Content-Type", $@"text/xml; charset=""{this.encoding.WebName}""");
        Headers.Add("SOAPACTION", $@"""{soapEnvelope.Schema}#{soapEnvelope.Action}""");
    }

    #region Overrides of HttpContent

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
    {
        if (memoryStream is null)
            return soapEnvelope.WriteAsync(stream, encoding);

        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream.CopyToAsync(stream);
    }

    protected override bool TryComputeLength(out long length)
    {
        if (memoryStream is not null)
        {
            length = memoryStream.Length;
            return true;
        }

        length = -1;
        return false;
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            memoryStream?.Dispose();
        }
        finally
        {
            base.Dispose(disposing);
        }
    }

    #endregion
}