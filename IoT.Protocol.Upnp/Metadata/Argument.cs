namespace IoT.Protocol.Upnp.Metadata;

public class Argument
{
    public Argument(string name, string direction, bool isRetVal, StateVariable stateVariable)
    {
        Name = name;
        Direction = direction;
        IsRetVal = isRetVal;
        StateVariable = stateVariable;
    }

    public string Name { get; }
    public string Direction { get; }
    public StateVariable StateVariable { get; }
    public bool IsRetVal { get; }
}