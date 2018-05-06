using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Soap
{
    public class SoapEnvelope
    {
        private const string CannotBeEmptyErrorMessage = "Cannot be null or empty or whitespace";
        private const string NS = "http://schemas.xmlsoap.org/soap/envelope/";
        private const string Prefix = "s";

        public string Action { get; }
        public string Schema { get; }
        public IDictionary<string, object> Arguments { get; }

        public SoapEnvelope(string action, string schema, IDictionary<string, object> args = null)
        {
            if(string.IsNullOrWhiteSpace(action)) throw new System.ArgumentException(CannotBeEmptyErrorMessage, nameof(action));
            if(string.IsNullOrWhiteSpace(schema)) throw new System.ArgumentException(CannotBeEmptyErrorMessage, nameof(schema));

            Schema = schema;
            Arguments = args ?? new Dictionary<string, object>();
            Action = action;
        }

        public void Serialize(Stream stream, Encoding encoding = null)
        {
            using(XmlWriter w = XmlWriter.Create(stream, new XmlWriterSettings() { Encoding = encoding ?? Encoding.UTF8 }))
            {
                w.WriteStartElement(Prefix, "Envelope", NS);
                w.WriteAttributeString(Prefix, "encodingStyle", NS, "http://schemas.xmlsoap.org/soap/encoding/");
                w.WriteStartElement(Prefix, "Body", NS);
                w.WriteStartElement("u", Action, Schema);
                if(Arguments != null)
                {
                    foreach(var p in Arguments)
                    {
                        w.WriteElementString(null, p.Key, null, Convert.ToString(p.Value));
                    }
                }
                w.WriteEndElement();
                w.WriteEndElement();
                w.WriteEndElement();
            };
        }

        public static SoapEnvelope Deserialize(Stream stream)
        {
            using(var sr = new StreamReader(stream, Encoding.UTF8))
                return Deserialize(sr);
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
                            var args = new Dictionary<string, object>();

                            if(!r.IsEmptyElement)
                            {
                                string argName = null;
                                while(r.Read() && (r.NodeType != EndElement || r.LocalName != name))
                                {
                                    switch(r.NodeType)
                                    {
                                        case Element: argName = r.LocalName; break;
                                        case EndElement: argName = null; break;
                                        case Text: case CDATA: if(argName != null) args[argName] = r.Value; break;
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