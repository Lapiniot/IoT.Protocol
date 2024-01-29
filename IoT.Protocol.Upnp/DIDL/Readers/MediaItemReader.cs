using System.Diagnostics.CodeAnalysis;
using System.Xml;
using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Upnp.DIDL.Readers;

public class MediaItemReader(bool parseResourceProps, bool parseVendorProps) : ItemReader<MediaItem>(parseResourceProps, parseVendorProps)
{
    #region Overrides of ItemReader<MediaItem>

    protected override MediaItem CreateElement(string id, string parentId, bool restricted) => new(id, parentId, restricted);

    protected override bool TryReadChildNode([NotNull] XmlReader reader, [NotNull] MediaItem element)
    {
        if (reader.NodeType != Element) return base.TryReadChildNode(reader, element);

        switch (reader.NamespaceURI)
        {
            case DC:
                switch (reader.LocalName)
                {
                    case "creator":
                        element.Creator = reader.ReadElementContentAsString();
                        return true;
                    case "date":
                        element.Date = reader.ReadElementContentAsDateTime();
                        return true;
                    case "description":
                        element.Description = reader.ReadElementContentAsString();
                        return true;
                    case "publisher":
                        (element.Publishers ??= []).Add(reader.ReadElementContentAsString());
                        return true;
                }

                break;
            case UPNP:
                switch (reader.LocalName)
                {
                    case "artist":
                        (element.Artists ??= []).Add(reader.ReadElementContentAsString());
                        return true;
                    case "album":
                        element.Album = reader.ReadElementContentAsString();
                        return true;
                    case "artistDiscographyURI":
                        element.DiscographyUrl = reader.ReadElementContentAsString();
                        return true;
                    case "lyricsURI":
                        element.LyricsUrl = reader.ReadElementContentAsString();
                        return true;
                    case "genre":
                        (element.Genres ??= []).Add(reader.ReadElementContentAsString());
                        return true;
                    case "originalTrackNumber":
                        element.TrackNumber = reader.ReadElementContentAsInt();
                        return true;
                    case "actor":
                        (element.Actors ??= []).Add(reader.ReadElementContentAsString());
                        return true;
                    case "author":
                        (element.Authors ??= []).Add(reader.ReadElementContentAsString());
                        return true;
                    case "producer":
                        (element.Producers ??= []).Add(reader.ReadElementContentAsString());
                        return true;
                    case "director":
                        (element.Directors ??= []).Add(reader.ReadElementContentAsString());
                        return true;
                }

                break;
        }

        return base.TryReadChildNode(reader, element);
    }

    #endregion
}