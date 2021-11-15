using System.Xml;

using static System.Xml.XmlNodeType;

namespace IoT.Protocol.Upnp.Metadata;

public static class ServiceDescriptionReader
{
    public const string NS = "urn:schemas-upnp-org:service-1-0";

    public static async Task<ServiceDescription> ReadAsync(Stream stream, CancellationToken cancellationToken)
    {
        Version version;
        IEnumerable<ServiceAction> actions = null;
        IReadOnlyDictionary<string, StateVariable> stateTable = null;

        using var reader = XmlReader.Create(stream, new() { Async = true, IgnoreComments = true, IgnoreWhitespace = true, IgnoreProcessingInstructions = true });

        while(await reader.ReadAsync().WaitAsync(cancellationToken).ConfigureAwait(false) && reader.Depth == 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(reader.NodeType != Element || reader.LocalName != "scpd" || reader.NamespaceURI != NS) continue;

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
                        case "actionList":
                            actions = await ReadActionListNodeAsync(reader, cancellationToken).ConfigureAwait(false);
                            break;
                        case "serviceStateTable":
                            stateTable = await ReadServiceStateTableNodeAsync(reader, cancellationToken).ConfigureAwait(false);
                            break;
                        default:
                            await reader.SkipAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                            break;
                    }
                }
            }
        }

        return new ServiceDescription(actions, stateTable);
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

    private static async Task<IEnumerable<ServiceAction>> ReadActionListNodeAsync(XmlReader reader, CancellationToken cancellationToken)
    {
        var actions = new List<ServiceAction>();

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
                    case "action":
                        actions.Add(await ReadActionNodeAsync(reader, cancellationToken).ConfigureAwait(false));
                        break;
                    default:
                        await reader.SkipAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                }
            }
        }

        return actions;
    }

    private static async Task<ServiceAction> ReadActionNodeAsync(XmlReader reader, CancellationToken cancellationToken)
    {
        string name = null;
        IEnumerable<Argument> arguments = null;

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
                    case "name":
                        name = await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    case "argumentList":
                        arguments = await ReadArgumentListNodeAsync(reader, cancellationToken).ConfigureAwait(false);
                        break;
                    default:
                        await reader.SkipAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                }
            }
        }

        return new ServiceAction(name, arguments);
    }

    private static async Task<IEnumerable<Argument>> ReadArgumentListNodeAsync(XmlReader reader, CancellationToken cancellationToken)
    {
        var arguments = new List<Argument>();

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
                    case "argument":
                        arguments.Add(await ReadArgumentNodeAsync(reader, cancellationToken).ConfigureAwait(false));
                        break;
                    default:
                        await reader.SkipAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                }
            }
        }

        return arguments;
    }

    private static async Task<Argument> ReadArgumentNodeAsync(XmlReader reader, CancellationToken cancellationToken)
    {
        string name = null, relatedStateVar = null;
        ArgumentDirection direction = default;
        bool isRetVal = false;

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
                    case "name":
                        name = await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    case "direction":
                        var value = await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        direction = value == "in" ? ArgumentDirection.In : ArgumentDirection.Out;
                        break;
                    case "relatedStateVariable":
                        relatedStateVar = await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    case "retval":
                        isRetVal = true;
                        break;
                    default:
                        await reader.SkipAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                }
            }
        }

        return new Argument(name, direction, isRetVal, relatedStateVar);
    }

    private static async Task<IReadOnlyDictionary<string, StateVariable>> ReadServiceStateTableNodeAsync(XmlReader reader, CancellationToken cancellationToken)
    {
        var table = new Dictionary<string, StateVariable>();

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
                    case "stateVariable":
                        var stateVar = await ReadStateVariableNodeAsync(reader, cancellationToken).ConfigureAwait(false);
                        table[stateVar.Name] = stateVar;
                        break;
                    default:
                        await reader.SkipAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                }
            }
        }

        return table;
    }

    private static async Task<StateVariable> ReadStateVariableNodeAsync(XmlReader reader, CancellationToken cancellationToken)
    {
        string name = null, dataType = null, defaultValue = null;
        bool sendEvents = false;
        IEnumerable<string> allowedValues = null;
        ArgumentValueRange valueRange = null;

        var depth = reader.Depth + 1;

        if(reader.HasAttributes && reader.MoveToAttribute("sendEvents"))
        {
            sendEvents = await reader.ReadContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false) == "yes";
        }

        while(await reader.ReadAsync().WaitAsync(cancellationToken).ConfigureAwait(false) && reader.Depth >= depth)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(reader.NodeType != Element) continue;

            while(reader.Depth == depth)
            {
                cancellationToken.ThrowIfCancellationRequested();

                switch(reader.LocalName)
                {
                    case "name":
                        name = await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    case "dataType":
                        dataType = reader.HasAttributes && reader.MoveToAttribute("type")
                            ? await reader.ReadContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false)
                            : await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    case "defaultValue":
                        defaultValue = await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    case "allowedValueList":
                        allowedValues = await ReadAllowedValueListNodeAsync(reader, cancellationToken).ConfigureAwait(false);
                        break;
                    case "allowedValueRange":
                        valueRange = await ReadAllowedValueRangeNodeAsync(reader, cancellationToken).ConfigureAwait(false);
                        break;
                    default:
                        await reader.SkipAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                }
            }
        }

        return new StateVariable(name, dataType, defaultValue, sendEvents, allowedValues, valueRange);
    }

    private static async Task<IEnumerable<string>> ReadAllowedValueListNodeAsync(XmlReader reader, CancellationToken cancellationToken)
    {
        var values = new List<string>();

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
                    case "allowedValue":
                        values.Add(await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false));
                        break;
                    default:
                        await reader.SkipAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                }
            }
        }

        return values;
    }

    private static async Task<ArgumentValueRange> ReadAllowedValueRangeNodeAsync(XmlReader reader, CancellationToken cancellationToken)
    {
        string minimum = null, maximum = null, step = null;

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
                    case "minimum":
                        minimum = await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    case "maximum":
                        maximum = await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    case "step":
                        step = await reader.ReadElementContentAsStringAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                    default:
                        await reader.SkipAsync().WaitAsync(cancellationToken).ConfigureAwait(false);
                        break;
                }
            }
        }

        return new ArgumentValueRange(minimum, maximum, step);
    }
}