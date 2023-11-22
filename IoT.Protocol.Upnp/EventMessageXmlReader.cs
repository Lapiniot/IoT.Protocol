using System.Xml;

using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Upnp;

public record struct EventMessage(string Namespace, IReadOnlyDictionary<string, string> Properties, IReadOnlyDictionary<string, string> VendorProperties);

public static class EventMessageXmlReader
{
    private const string EventNS = "urn:schemas-upnp-org:event-1-0";

    public static async Task<string> ReadLastChangeContentAsync(XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        while (await reader.ReadAsync().ConfigureAwait(false) && reader.Depth == 0)
        {
            if (reader.NodeType != Element || reader.LocalName != "propertyset" || reader.NamespaceURI != EventNS) continue;

            while (await reader.ReadAsync().ConfigureAwait(false) && reader.Depth == 1)
            {
                if (reader.NodeType != Element || reader.LocalName != "property" || reader.NamespaceURI != EventNS) continue;

                while (await reader.ReadAsync().ConfigureAwait(false) && reader.Depth == 2)
                {
                    if (reader.NodeType != Element || reader.Name != "LastChange") continue;
                    return await reader.ReadElementContentAsStringAsync().ConfigureAwait(false);
                }
            }
        }

        return null;
    }

    public static async Task<EventMessage> ReadAsync(XmlReader reader)
    {
        var content = await ReadLastChangeContentAsync(reader).ConfigureAwait(false);

        if (string.IsNullOrEmpty(content)) return default;

        string eventNamespace = null;
        var metadata = new Dictionary<string, string>();
        var vendor = new Dictionary<string, string>();

        using var xr = XmlReader.Create(new StringReader(content),
            new()
            {
                CloseInput = true,
                ConformanceLevel = ConformanceLevel.Fragment,
                IgnoreProcessingInstructions = true,
                IgnoreWhitespace = true,
                IgnoreComments = true
            });

        while (xr.Read() && xr.Depth == 0)
        {
            if (xr.NodeType != Element || xr.LocalName != "Event") continue;

            eventNamespace = xr.NamespaceURI;

            while (xr.Read() && xr.Depth == 1)
            {
                if (xr.NodeType != Element && xr.LocalName != "InstanceID" || xr.NamespaceURI != eventNamespace) continue;

                while (xr.Read() && xr.Depth == 2)
                {
                    if (xr.NodeType != Element) continue;
#pragma warning disable CA1849 // Call async methods when in an async method
                    if (xr.NamespaceURI == eventNamespace)
                    {
                        metadata[xr.LocalName] = xr.MoveToAttribute("val") ? xr.ReadContentAsString() : xr.ReadElementContentAsString();
                    }
                    else
                    {
                        vendor[xr.Name] = xr.MoveToAttribute("val") ? xr.ReadContentAsString() : xr.ReadElementContentAsString();
                    }
#pragma warning restore CA1849 // Call async methods when in an async method
                }
            }

            break;
        }

        return new(eventNamespace, metadata, vendor);
    }
}