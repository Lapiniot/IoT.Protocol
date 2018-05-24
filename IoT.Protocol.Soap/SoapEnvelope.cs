using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using static System.String;
using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Soap
{
    public class SoapEnvelope
    {
        private const string CannotBeEmptyErrorMessage = "Cannot be null or empty or whitespace";
        private const string NS = "http://schemas.xmlsoap.org/soap/envelope/";
        private const string Prefix = "s";

        public SoapEnvelope(string action, string schema, IDictionary<string, string> args = null)
        {
            if(IsNullOrWhiteSpace(action)) throw new ArgumentException(CannotBeEmptyErrorMessage, nameof(action));
            if(IsNullOrWhiteSpace(schema)) throw new ArgumentException(CannotBeEmptyErrorMessage, nameof(schema));

            Schema = schema;
            Arguments = args ?? new Dictionary<string, string>();
            Action = action;
        }

        public SoapEnvelope(string action, string schema, params (string name, object value)[] args) :
            this(action, schema, args.ToDictionary(a => a.name, a => Convert.ToString(a.value)))
        {
        }

        public string Action { get; }
        public string Schema { get; }
        public IDictionary<string, string> Arguments { get; }

        public string this[string name]
        {
            get { return Arguments[name]; }
        }

        public override string ToString()
        {
            return $"{Schema}#{Action}: {{{Join(", ", Arguments.Select(a => $"{a.Key} = {a.Value}"))}}}";
        }

        public void Serialize(Stream stream, Encoding encoding = null)
        {
            using(var w = XmlWriter.Create(stream, new XmlWriterSettings {Encoding = encoding ?? Encoding.UTF8}))
            {
                w.WriteStartElement(Prefix, "Envelope", NS);
                w.WriteAttributeString(Prefix, "encodingStyle", NS, "http://schemas.xmlsoap.org/soap/encoding/");
                w.WriteStartElement(Prefix, "Body", NS);
                w.WriteStartElement("u", Action, Schema);
                if(Arguments != null)
                {
                    foreach(var p in Arguments)
                    {
                        w.WriteElementString(Empty, p.Key, Empty, Convert.ToString(p.Value));
                    }
                }

                w.WriteEndElement();
                w.WriteEndElement();
                w.WriteEndElement();
            }
        }

        public static SoapEnvelope Deserialize(Stream stream)
        {
            using(var sr = new StreamReader(stream, Encoding.UTF8))
            {
                return Deserialize(sr);
            }
        }

        public static SoapEnvelope Deserialize(TextReader textReader)
        {
            using(var r = XmlReader.Create(textReader))
            {
                if(r.Read() && r.NodeType == XmlNodeType.XmlDeclaration)
                {
                    if(r.MoveToContent() == Element && r.LocalName == "Envelope" && r.NamespaceURI == NS)
                    {
                        if(r.ReadToDescendant("Body", NS) && r.Read())
                        {
                            r.MoveToElement();

                            var name = r.LocalName;
                            var schema = r.NamespaceURI;
                            var args = new Dictionary<string, string>();

                            if(!r.IsEmptyElement)
                            {
                                string argName = null;
                                while(r.Read() && (r.NodeType != EndElement || r.LocalName != name))
                                {
                                    switch(r.NodeType)
                                    {
                                        case Element:
                                            argName = r.LocalName;
                                            break;
                                        case EndElement:
                                            argName = null;
                                            break;
                                        case Text:
                                        case CDATA:
                                            if(argName != null) args[argName] = r.Value;
                                            break;
                                    }
                                }
                            }

                            return new SoapEnvelope(name, schema, args);
                        }
                    }
                }

                throw new InvalidDataException("Invalid XML data");
            }
        }
    }
}