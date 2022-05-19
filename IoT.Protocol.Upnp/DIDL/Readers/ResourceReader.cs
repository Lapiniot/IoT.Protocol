using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml;
using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Upnp.DIDL.Readers;

public class ResourceReader : ReaderBase<Resource>
{
    private static ResourceReader instance;

    public static ResourceReader Instance => instance ??= new();

    #region Overrides of ReaderBase<Resource>

    protected override bool TryReadChildNode([NotNull] XmlReader reader, [NotNull] Resource element)
    {
        if (reader.NodeType is not CDATA and not Text) return false;
        element.Url = reader.ReadContentAsString();
        return true;
    }

    protected override Resource CreateElement([NotNull] XmlReader reader)
    {
        var resource = new Resource();

        if (reader.AttributeCount > 0)
        {
            for (var i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);

                switch (reader.Name)
                {
                    case "protocolInfo":
                        resource.Protocol = reader.Value;
                        break;
                    case "size":
                        resource.Size = reader.ReadContentAsLong();
                        break;
                    case "duration":
                        if (TimeSpan.TryParse(reader.ReadContentAsString(), CultureInfo.InvariantCulture, out var value))
                        {
                            resource.Duration = value;
                        }

                        break;
                    case "bitrate":
                        resource.BitRate = reader.ReadContentAsInt();
                        break;
                    case "sampleFrequency":
                        resource.SampleFrequency = reader.ReadContentAsInt();
                        break;
                    case "bitsPerSample":
                        resource.BitsPerSample = reader.ReadContentAsInt();
                        break;
                    case "nrAudioChannels":
                        resource.NrAudioChannels = reader.ReadContentAsInt();
                        break;
                    case "resolution":
                        resource.Resolution = reader.ReadContentAsString();
                        break;
                    case "colorDepth":
                        resource.ColorDepth = reader.ReadContentAsInt();
                        break;
                    case "contentInfoURI":
                        resource.ContentInfoUrl = reader.ReadContentAsString();
                        break;
                    case "protection":
                        resource.Protection = reader.ReadContentAsString();
                        break;
                    default:
                        resource.Attributes ??= new();
                        resource.Attributes[reader.Name] = reader.Value;
                        break;
                }
            }
        }

        _ = reader.MoveToElement();

        return resource;
    }

    #endregion
}