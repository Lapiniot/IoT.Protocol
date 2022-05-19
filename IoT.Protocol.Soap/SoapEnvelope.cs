using System.Text;
using System.Xml;
using static System.String;
using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Soap;

public record SoapEnvelope
{
    private const string CannotBeEmptyErrorMessage = "Cannot be null or empty or whitespace";
    private const string SoapEnvelopeNs = "http://schemas.xmlsoap.org/soap/envelope/";
    private const string SoapEncodingNs = "http://schemas.xmlsoap.org/soap/encoding/";
    private const string Prefix = "s";

    public SoapEnvelope(string action, string schema, IReadOnlyDictionary<string, string> args = null)
    {
        if (IsNullOrWhiteSpace(action)) throw new ArgumentException(CannotBeEmptyErrorMessage, nameof(action));
        if (IsNullOrWhiteSpace(schema)) throw new ArgumentException(CannotBeEmptyErrorMessage, nameof(schema));

        Schema = schema;
        Arguments = args ?? new Dictionary<string, string>();
        Action = action;
    }

    public string Action { get; init; }
    public string Schema { get; init; }
    public IReadOnlyDictionary<string, string> Arguments { get; }
    public string this[string name] => Arguments[name];

    public override string ToString() =>
        $"{Schema}#{Action}: {{{Join(", ", Arguments.Select(a => $"{a.Key} = {a.Value}"))}}}";

    public async Task WriteAsync(Stream stream, Encoding encoding)
    {
        await using var writer = XmlWriter.Create(stream, new()
        {
            Encoding = encoding ?? Encoding.UTF8,
            CloseOutput = false,
            OmitXmlDeclaration = true,
            Async = true
        });

        await WriteAsync(writer).ConfigureAwait(false);
    }

    public async Task WriteAsync(TextWriter textWriter)
    {
        await using var writer = XmlWriter.Create(textWriter, new()
        {
            CloseOutput = false,
            OmitXmlDeclaration = true,
            Async = true
        });

        await WriteAsync(writer).ConfigureAwait(false);
    }

    public async Task WriteAsync(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        var task = writer.WriteStartElementAsync(Prefix, "Envelope", SoapEnvelopeNs);
        if (!task.IsCompletedSuccessfully) await task.ConfigureAwait(false);
        task = writer.WriteAttributeStringAsync(Prefix, "encodingStyle", SoapEnvelopeNs, SoapEncodingNs);
        if (!task.IsCompletedSuccessfully) await task.ConfigureAwait(false);
        task = writer.WriteStartElementAsync(Prefix, "Body", SoapEnvelopeNs);
        if (!task.IsCompletedSuccessfully) await task.ConfigureAwait(false);
        task = writer.WriteStartElementAsync("u", Action, Schema);
        if (!task.IsCompletedSuccessfully) await task.ConfigureAwait(false);

        if (Arguments != null)
        {
            foreach (var (key, value) in Arguments)
            {
                task = writer.WriteElementStringAsync(Empty, key, Empty, value ?? Empty);
                if (!task.IsCompletedSuccessfully) await task.ConfigureAwait(false);
            }
        }

        task = writer.WriteEndDocumentAsync();
        if (!task.IsCompletedSuccessfully) await task.ConfigureAwait(false);
        task = writer.FlushAsync();
        if (!task.IsCompletedSuccessfully) await task.ConfigureAwait(false);
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

    public void Write(TextWriter textWriter)
    {
        using var writer = XmlWriter.Create(textWriter, new()
        {
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

        _ = reader.MoveToContent();

        var name = reader.LocalName;
        var schema = reader.NamespaceURI;
        var depth = reader.Depth;
        var args = new Dictionary<string, string>();

        if (reader.IsEmptyElement) return new(name, schema, args);

        _ = reader.Read();

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
                _ = reader.Read();
            }
        }

        return new(name, schema, args);
    }
}