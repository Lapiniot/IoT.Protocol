using System.Xml;
using IoT.Protocol.Upnp.DIDL.Readers;
using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Upnp.DIDL;

public static class DIDLXmlReader
{
    private const string Ns = "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/";

    private static readonly XmlReaderSettings Settings = new()
    {
        CloseInput = true,
        ConformanceLevel = ConformanceLevel.Fragment,
        IgnoreComments = true,
        IgnoreProcessingInstructions = true,
        IgnoreWhitespace = true
    };

    public static IEnumerable<Item> Read(string content, bool readResourceProps, bool readVendorProps)
    {
        var containerReader = new ContainerItemReader(readResourceProps, readVendorProps);
        var mediaItemReader = new MediaItemReader(readResourceProps, readVendorProps);

        using var reader = XmlReader.Create(new StringReader(content), Settings);

        if (!reader.ReadToFollowing("DIDL-Lite", Ns)) yield break;

        var depth = reader.Depth;

        while (reader.Read() && reader.Depth > depth)
        {
            if (reader.NodeType != Element || reader.NamespaceURI != Ns) continue;

            switch (reader.Name)
            {
                case "container":
                    yield return containerReader.Read(reader);
                    break;
                case "item":
                    yield return mediaItemReader.Read(reader);
                    break;
            }
        }
    }
}