using System.Xml;

using static System.UriKind;
using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Upnp;

public static class DeviceDescriptionXmlReader
{
    public static readonly string NS = "urn:schemas-upnp-org:device-1-0";

    public static async Task<DeviceDescription> ReadAsync(Stream input, CancellationToken cancellationToken)
    {
        DeviceDescription description = null;
        Version version = null;

        using var reader = XmlReader.Create(input, new() { Async = true, IgnoreComments = true, IgnoreWhitespace = true, IgnoreProcessingInstructions = true });

        while(await reader.ReadAsync().WaitAsync(cancellationToken).ConfigureAwait(false) && reader.Depth == 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(reader.NodeType != Element || reader.LocalName != "root" || reader.NamespaceURI != NS) continue;

            while(await reader.ReadAsync().WaitAsync(cancellationToken).ConfigureAwait(false) && reader.Depth == 1)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if(reader.NodeType != Element) continue;

                while(reader.Depth == 1)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    switch(reader.LocalName)
                    {
                        case "specVersion":
                            version = await ReadVersionNodeAsync(reader, cancellationToken).ConfigureAwait(false);
                            break;
                        case "device":
                            description = await ReadDeviceNodeAsync(reader, cancellationToken).ConfigureAwait(false);
                            break;
                        default:
                            await reader.SkipAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                            break;
                    }
                }
            }
        }

        if(description is null)
        {
            throw new InvalidDataException("Mandatory <device> node cannot be read");
        }

        description.Version = version;

        return description;
    }

    private static async Task<Version> ReadVersionNodeAsync(XmlReader reader, CancellationToken cancellationToken)
    {
        int major = 0;
        int minor = 0;
        var depth = reader.Depth + 1;

        while(await reader.ReadAsync().WaitAsync(cancellationToken).ConfigureAwait(false) && reader.Depth >= depth)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(reader.NodeType != Element) continue;

            while(reader.Depth == depth)
            {
                cancellationToken.ThrowIfCancellationRequested();

                switch(reader.LocalName)
                {
                    case "major":
                        major = reader.ReadElementContentAsInt();
                        break;
                    case "minor":
                        minor = reader.ReadElementContentAsInt();
                        break;
                    default:
                        await reader.SkipAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                }
            }
        }

        return new Version(major, minor);
    }

    private static async Task<DeviceDescription> ReadDeviceNodeAsync(XmlReader reader, CancellationToken cancellationToken)
    {
        string deviceType = null, friendlyName = null, serialNumber = null, udn = null, manufacturer = null;
        string modelDescription = null, modelName = null, modelNumber = null;
        IEnumerable<ServiceDescription> services = null;
        IEnumerable<IconDescription> icons = null;
        Uri manufacturerUrl = null, modelUrl = null, presentationUrl = null;

        var depth = reader.Depth + 1;

        while(await reader.ReadAsync().WaitAsync(cancellationToken).ConfigureAwait(false) && reader.Depth >= depth)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(reader.NodeType != Element) continue;

            while(reader.Depth == depth)
            {
                cancellationToken.ThrowIfCancellationRequested();

                switch(reader.LocalName)
                {
                    case "deviceType":
                        deviceType = await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    case "friendlyName":
                        friendlyName = await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    case "manufacturer":
                        manufacturer = await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    case "manufacturerURL":
                        manufacturerUrl = new(await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false), RelativeOrAbsolute);
                        break;
                    case "modelDescription":
                        modelDescription = await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    case "modelName":
                        modelName = await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    case "modelNumber":
                        modelNumber = await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    case "modelURL":
                        modelUrl = new(await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false), RelativeOrAbsolute);
                        break;
                    case "serialNumber":
                        serialNumber = await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    case "UDN":
                        udn = await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    case "presentationURL":
                        presentationUrl = new(await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false), RelativeOrAbsolute);
                        break;
                    case "iconList":
                        icons = await ReadIconListNodeAsync(reader, cancellationToken).ConfigureAwait(false);
                        break;
                    case "serviceList":
                        services = await ReadServiceListNodeAsync(reader, cancellationToken).ConfigureAwait(false);
                        break;
                    default:
                        await reader.SkipAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                }
            }
        }

        return new DeviceDescription(udn, deviceType, friendlyName, serialNumber, presentationUrl,
            manufacturer, manufacturerUrl, modelName, modelDescription, modelNumber, modelUrl, icons, services);
    }

    private static async Task<IEnumerable<ServiceDescription>> ReadServiceListNodeAsync(XmlReader reader, CancellationToken cancellationToken)
    {
        var list = new List<ServiceDescription>();

        var depth = reader.Depth + 1;

        while(await reader.ReadAsync().WaitAsync(cancellationToken).ConfigureAwait(false) && reader.Depth >= depth)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(reader.NodeType != Element) continue;

            while(reader.Depth == depth)
            {
                cancellationToken.ThrowIfCancellationRequested();

                switch(reader.LocalName)
                {
                    case "service":
                        list.Add(await ReadServiceNodeAsync(reader, cancellationToken).ConfigureAwait(false));
                        break;
                    default:
                        await reader.SkipAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                }
            }
        }

        return list;
    }

    private static async Task<ServiceDescription> ReadServiceNodeAsync(XmlReader reader, CancellationToken cancellationToken)
    {
        string serviceType = null, serviceId = null;
        Uri metadataUrl = null, controlUrl = null, eventSubUrl = null;

        var depth = reader.Depth + 1;

        while(await reader.ReadAsync().WaitAsync(cancellationToken).ConfigureAwait(false) && reader.Depth >= depth)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(reader.NodeType != Element) continue;

            while(reader.Depth == depth)
            {
                cancellationToken.ThrowIfCancellationRequested();

                switch(reader.LocalName)
                {
                    case "serviceType":
                        serviceType = await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    case "serviceId":
                        serviceId = await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    case "controlURL":
                        controlUrl = new(await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false), RelativeOrAbsolute);
                        break;
                    case "eventSubURL":
                        eventSubUrl = new(await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false), RelativeOrAbsolute);
                        break;
                    case "SCPDURL":
                        metadataUrl = new(await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false), RelativeOrAbsolute);
                        break;
                    default:
                        await reader.SkipAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                }
            }
        }

        return new(serviceType, serviceId, metadataUrl, controlUrl, eventSubUrl);
    }

    private static async Task<IEnumerable<IconDescription>> ReadIconListNodeAsync(XmlReader reader, CancellationToken cancellationToken)
    {
        var list = new List<IconDescription>();

        var depth = reader.Depth + 1;

        while(await reader.ReadAsync().WaitAsync(cancellationToken).ConfigureAwait(false) && reader.Depth >= depth)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(reader.NodeType != Element) continue;

            while(reader.Depth == depth)
            {
                cancellationToken.ThrowIfCancellationRequested();

                switch(reader.LocalName)
                {
                    case "icon":
                        list.Add(await ReadIconNodeAsync(reader, cancellationToken).ConfigureAwait(false));
                        break;
                    default:
                        await reader.SkipAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                }
            }
        }

        return list;
    }

    private static async Task<IconDescription> ReadIconNodeAsync(XmlReader reader, CancellationToken cancellationToken)
    {
        int width = 0, height = 0, colorDepth = 0;
        string mimeType = null;
        Uri url = null;

        var depth = reader.Depth + 1;

        while(await reader.ReadAsync().WaitAsync(cancellationToken).ConfigureAwait(false) && reader.Depth >= depth)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(reader.NodeType != Element) continue;

            while(reader.Depth == depth)
            {
                cancellationToken.ThrowIfCancellationRequested();

                switch(reader.LocalName)
                {
                    case "mimetype":
                        mimeType = await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    case "width":
                        width = reader.ReadElementContentAsInt();
                        break;
                    case "height":
                        height = reader.ReadElementContentAsInt();
                        break;
                    case "depth":
                        colorDepth = reader.ReadElementContentAsInt();
                        break;
                    case "url":
                        url = new(await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false), RelativeOrAbsolute);
                        break;
                    default:
                        await reader.SkipAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                }
            }
        }

        return new(url, mimeType, colorDepth, width, height);
    }
}