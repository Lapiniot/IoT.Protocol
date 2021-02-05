using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace IoT.Protocol.Upnp.DIDL
{
    public static class DIDLUtils
    {
        private const string XmlnsNamespace = "http://www.w3.org/2000/xmlns/";
        private const string DIDLLiteNamespace = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/";
        private const string DCNamespace = "http://purl.org/dc/elements/1.1/";
        private const string UPNPNamespace = "urn:schemas-upnp-org:metadata-1-0/upnp/";
        private const string DLNANamespace = "urn:schemas-dlna-org:metadata-1-0/";

        public static XmlWriter CreateDidlXmlWriter(StringBuilder sb)
        {
            var writer = XmlWriter.Create(sb, new XmlWriterSettings() { OmitXmlDeclaration = true, WriteEndDocumentOnClose = true });
            writer.WriteStartElement("DIDL-Lite", DIDLLiteNamespace);
            writer.WriteAttributeString("dc", XmlnsNamespace, DCNamespace);
            writer.WriteAttributeString("upnp", XmlnsNamespace, UPNPNamespace);
            writer.WriteAttributeString("dlna", XmlnsNamespace, DLNANamespace);
            return writer;
        }

        public static void WriteItem(XmlWriter writer, string title, string description, string genre, Uri url, long? length, string contentType, int? br)
        {
            writer.WriteStartElement("item");
            writer.WriteElementString("title", DCNamespace, title);
            if(!string.IsNullOrEmpty(description)) writer.WriteElementString("description", DCNamespace, description);
            if(!string.IsNullOrEmpty(genre)) writer.WriteElementString("genre", UPNPNamespace, genre);
            writer.WriteElementString("class", UPNPNamespace, "object.item.audioItem.musicTrack");
            writer.WriteStartElement("res");
            if(length is not null) writer.WriteAttributeString("size", length.Value.ToString(CultureInfo.InvariantCulture));
            if(br is not null) writer.WriteAttributeString("bitrate", br.Value.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("protocolInfo", $"http-get:*:{(contentType ?? "audio/mpegurl")}:*");
            writer.WriteValue(url.AbsoluteUri);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public static void CopyItems(string metadata, XmlWriter writer)
        {
            using(var input = new StringReader(metadata))
            using(var reader = XmlReader.Create(input))
            {
                if(!reader.ReadToDescendant("DIDL-Lite") || reader.NamespaceURI != DIDLUtils.DIDLLiteNamespace) return;
                if(!reader.ReadToDescendant("item") || reader.NamespaceURI != DIDLUtils.DIDLLiteNamespace) return;
                writer.WriteNode(reader, true);
            }
        }

        public static void CopyItems(string metadata, XmlWriter writer, Stack<(string Id, int Depth)> containerIds, int depth)
        {
            if(string.IsNullOrEmpty(metadata)) return;
            if(writer is null) throw new ArgumentNullException(nameof(writer));
            if(containerIds is null) throw new ArgumentNullException(nameof(containerIds));

            using(var input = new StringReader(metadata))
            using(var reader = XmlReader.Create(input))
            {
                if(!reader.ReadToDescendant("DIDL-Lite") || reader.NamespaceURI != DIDLLiteNamespace)
                {
                    return;
                }

                while(reader.Read() && reader.NodeType != XmlNodeType.Element) ;

                while(reader.NodeType != XmlNodeType.EndElement)
                {
                    if(reader.NamespaceURI == DIDLLiteNamespace)
                    {
                        switch(reader.Name)
                        {
                            case "item":
                                writer.WriteNode(reader, true);
                                break;
                            case "container":
                                var id = reader.GetAttribute("id");
                                if(!string.IsNullOrEmpty(id)) containerIds.Push((id, depth));
                                reader.Skip();
                                break;
                        }
                    }
                }
            }
        }
    }
}