using System.Text;
using System.Xml;
using static System.String;
using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Soap;

public record SoapEnvelope
{
    private const string SoapEnvelopeNs = "http://schemas.xmlsoap.org/soap/envelope/";
    private const string SoapEncodingNs = "http://schemas.xmlsoap.org/soap/encoding/";
    private const string Prefix = "s";

    public SoapEnvelope(string action, string schema, IReadOnlyDictionary<string, string> args = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(action);
        ArgumentException.ThrowIfNullOrEmpty(schema);

        Schema = schema;
        Arguments = args ?? new Dictionary<string, string>();
        Action = action;
    }

    public string Action { get; init; }
    public string Schema { get; init; }
    public IReadOnlyDictionary<string, string> Arguments { get; }
    public string this[string name] => Arguments[name];

    public override string ToString() => $"{Schema}#{Action}: {{{Join(", ", Arguments.Select(a => $"{a.Key} = {a.Value}"))}}}";

    public async Task WriteAsync(Stream stream, Encoding encoding)
    {
#pragma warning disable CA2000 // Dispose objects before losing scope
        var writer = XmlWriter.Create(stream, new()
        {
            Encoding = encoding ?? Encoding.UTF8,
            CloseOutput = false,
            OmitXmlDeclaration = true,
            Async = true
        });
#pragma warning restore CA2000 // Dispose objects before losing scope

        await using (writer.ConfigureAwait(false))
            await WriteAsync(writer).ConfigureAwait(false);
    }

    public async Task WriteAsync(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        await writer.WriteStartElementAsync(Prefix, "Envelope", SoapEnvelopeNs).ConfigureAwait(false);
        await writer.WriteAttributeStringAsync(Prefix, "encodingStyle", SoapEnvelopeNs, SoapEncodingNs).ConfigureAwait(false);
        await writer.WriteStartElementAsync(Prefix, "Body", SoapEnvelopeNs).ConfigureAwait(false);
        await writer.WriteStartElementAsync("u", Action, Schema).ConfigureAwait(false);

        if (Arguments != null)
        {
            foreach (var (key, value) in Arguments)
            {
                await writer.WriteElementStringAsync(Empty, key, Empty, value ?? Empty).ConfigureAwait(false);
            }
        }

        await writer.WriteEndDocumentAsync().ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);
    }

    public void Write(Stream stream, Encoding encoding)
    {
        using var writer = XmlWriter.Create(stream, new()
        {
            Encoding = encoding ?? Encoding.UTF8,
            CloseOutput = false,
            OmitXmlDeclaration = true,
            Async = true
        });

        Write(writer);
    }

    public void Write(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartElement(Prefix, "Envelope", SoapEnvelopeNs);
        writer.WriteAttributeString(Prefix, "encodingStyle", SoapEnvelopeNs, SoapEncodingNs);
        writer.WriteStartElement(Prefix, "Body", SoapEnvelopeNs);
        writer.WriteStartElement("u", Action, Schema);

        if (Arguments != null)
        {
            foreach (var (key, value) in Arguments)
            {
                writer.WriteElementString(Empty, key, Empty, value ?? Empty);
            }
        }

        writer.WriteEndDocument();
        writer.Flush();
    }

    public static SoapEnvelope Deserialize(TextReader textReader)
    {
        using var reader = XmlReader.Create(textReader);

        if (!(reader.ReadToDescendant("Envelope", SoapEnvelopeNs) && reader.ReadToDescendant("Body", SoapEnvelopeNs) && reader.Read()))
        {
            throw new InvalidDataException("Invalid XML data");
        }

        reader.MoveToContent();

        var name = reader.LocalName;
        var schema = reader.NamespaceURI;
        var depth = reader.Depth;
        var args = new Dictionary<string, string>();

        if (reader.IsEmptyElement) return new(name, schema, args);

        reader.Read();

        while (!reader.EOF && reader.Depth > depth)
        {
            if (reader.NodeType == Element)
            {
                var key = reader.LocalName;

                if (!reader.Read()) continue;
                var nt = reader.MoveToContent();
                if (nt is Text or CDATA)
                {
                    args[key] = reader.ReadContentAsString();
                }
            }
            else
            {
                reader.Read();
            }
        }

        return new(name, schema, args);
    }
}