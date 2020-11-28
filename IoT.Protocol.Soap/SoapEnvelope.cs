using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using static System.Globalization.CultureInfo;
using static System.String;
using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Soap
{
    public record SoapEnvelope
    {
        private const string CannotBeEmptyErrorMessage = "Cannot be null or empty or whitespace";
        private const string Ns = "http://schemas.xmlsoap.org/soap/envelope/";
        private const string Prefix = "s";

        public SoapEnvelope(string action, string schema, IDictionary<string, string> args = null)
        {
            if(IsNullOrWhiteSpace(action)) throw new ArgumentException(CannotBeEmptyErrorMessage, nameof(action));
            if(IsNullOrWhiteSpace(schema)) throw new ArgumentException(CannotBeEmptyErrorMessage, nameof(schema));

            Schema = schema;
            Arguments = args ?? new Dictionary<string, string>();
            Action = action;
        }

        public SoapEnvelope(string action, string schema, params (string Name, object Value)[] args) :
            this(action, schema, args.ToDictionary(a => a.Name, a => Convert.ToString(a.Value, InvariantCulture))) {}

        public string Action { get; init; }
        public string Schema { get; init; }
        public IDictionary<string, string> Arguments { get; }
        public string this[string name] => Arguments[name];

        public override string ToString()
        {
            return $"{Schema}#{Action}: {{{Join(", ", Arguments.Select(a => $"{a.Key} = {a.Value}"))}}}";
        }

        public void Serialize(Stream stream, Encoding encoding = null)
        {
            using var w = XmlWriter.Create(stream, new XmlWriterSettings { Encoding = encoding ?? Encoding.UTF8 });
            w.WriteStartElement(Prefix, "Envelope", Ns);
            w.WriteAttributeString(Prefix, "encodingStyle", Ns, "http://schemas.xmlsoap.org/soap/encoding/");
            w.WriteStartElement(Prefix, "Body", Ns);
            w.WriteStartElement("u", Action, Schema);
            
            if(Arguments != null)
            {
                foreach(var (key, value) in Arguments)
                {
                    w.WriteElementString(Empty, key, Empty, Convert.ToString(value, InvariantCulture));
                }
            }

            w.WriteEndElement();
            w.WriteEndElement();
            w.WriteEndElement();
        }

        public static SoapEnvelope Deserialize(TextReader textReader)
        {
            using var reader = XmlReader.Create(textReader);

            if(!(reader.ReadToDescendant("Envelope", Ns) && reader.ReadToDescendant("Body", Ns) && reader.Read()))
            {
                throw new InvalidDataException("Invalid XML data");
            }

            reader.MoveToContent();

            var name = reader.LocalName;
            var schema = reader.NamespaceURI;
            var depth = reader.Depth;

            var args = new Dictionary<string, string>();

            if(reader.IsEmptyElement) return new SoapEnvelope(name, schema, args);

            while(reader.Read() && reader.Depth > depth)
            {
                if(reader.NodeType == Element)
                {
                    args[reader.LocalName] = reader.ReadElementContentAsString();
                }
            }

            return new SoapEnvelope(name, schema, args);
        }
    }
}