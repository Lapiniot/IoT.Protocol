using System.Collections.Generic;
using System.Xml;
using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Upnp.DIDL.Readers
{
    public class MediaItemReader : ItemReader<MediaItem>
    {
        private static MediaItemReader instance;

        public static MediaItemReader Instance
        {
            get { return instance ?? (instance = new MediaItemReader()); }
        }

        #region Overrides of ItemReader<MediaItem>

        protected override MediaItem CreateElement(string id, string parentId, bool restricted)
        {
            return new MediaItem(id, parentId, restricted);
        }

        protected override bool TryReadChildNode(XmlReader reader, MediaItem element)
        {
            if(reader.NodeType == Element)
            {
                switch(reader.NamespaceURI)
                {
                    case DC:
                        {
                            switch(reader.LocalName)
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
                            }

                            break;
                        }
                    case UPNP:
                        {
                            switch(reader.LocalName)
                            {
                                case "artist":
                                    (element.Artists ?? (element.Artists = new List<string>())).Add(reader.ReadElementContentAsString());
                                    return true;
                                case "album":
                                    element.Album = reader.ReadElementContentAsString();
                                    return true;
                                case "artistDiscographyURI":
                                    element.DiscographyUri = reader.ReadElementContentAsString();
                                    return true;
                                case "lyricsURI":
                                    element.LyricsUri = reader.ReadElementContentAsString();
                                    return true;
                                case "genre":
                                    (element.Genres ?? (element.Genres = new List<string>())).Add(reader.ReadElementContentAsString());
                                    return true;
                                case "originalTrackNumber":
                                    element.TrackNumber = reader.ReadElementContentAsInt();
                                    return true;
                                case "actor":
                                    (element.Actors ?? (element.Actors = new List<string>())).Add(reader.ReadElementContentAsString());
                                    return true;
                                case "author":
                                    (element.Authors ?? (element.Authors = new List<string>())).Add(reader.ReadElementContentAsString());
                                    return true;
                                case "producer":
                                    (element.Producers ?? (element.Producers = new List<string>())).Add(reader.ReadElementContentAsString());
                                    return true;
                                case "publisher":
                                    (element.Publishers ?? (element.Publishers = new List<string>())).Add(reader.ReadElementContentAsString());
                                    return true;
                            }

                            break;
                        }
                }
            }

            return base.TryReadChildNode(reader, element);
        }

        #endregion
    }
}