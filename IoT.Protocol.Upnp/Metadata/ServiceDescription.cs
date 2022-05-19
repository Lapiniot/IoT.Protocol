namespace IoT.Protocol.Upnp.Metadata;

public record ServiceDescription(IEnumerable<ServiceAction> Actions, IReadOnlyDictionary<string, StateVariable> StateTable, Version Version);

public record ServiceAction(string Name, IEnumerable<Argument> Arguments);

public record Argument(string Name, ArgumentDirection Direction, bool IsRetVal, string RelatedStateVar);

public record StateVariable(string Name, string DataTypeName, string DefaultValue, bool SendEvents,
    IEnumerable<string> AllowedValues, ArgumentValueRange ValueRange)
{
    private static readonly IDictionary<string, Type> Map = new Dictionary<string, Type>
    {
        {"ui1", typeof(byte)},
        {"ui2", typeof(ushort)},
        {"ui4", typeof(uint)},
        {"ui8", typeof(ulong)},
        {"i1", typeof(sbyte)},
        {"i2", typeof(short)},
        {"i4", typeof(int)},
        {"i8", typeof(long)},
        {"int", typeof(int)},
        {"r4", typeof(float)},
        {"r8", typeof(double)},
        {"number", typeof(double)},
        {"fixed.14.4", typeof(double)},
        {"float", typeof(float)},
        {"char", typeof(char)},
        {"string", typeof(string)},
        {"date", typeof(DateOnly)},
        {"dateTime", typeof(DateTime)},
        {"dateTime.tz", typeof(DateTime)},
        {"time", typeof(TimeOnly)},
        {"time.tz", typeof(DateTime)},
        {"boolean", typeof(bool)},
        {"bin.base64", typeof(string)},
        {"bin.hex", typeof(string)},
        {"uri", typeof(Uri)},
        {"uuid", typeof(string)}
    };

    public Type GetRuntimeType() => Map.TryGetValue(Name, out var type) ? type : typeof(object);
}

public record ArgumentValueRange(string Minimum, string Maximum, string Step);

public enum ArgumentDirection
{
    In,
    Out
}