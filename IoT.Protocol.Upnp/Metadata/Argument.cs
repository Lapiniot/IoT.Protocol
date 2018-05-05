namespace IoT.Protocol.Upnp.Metadata
{
    public class Argument
    {
        public string Name { get; }
        public string Direction { get; }
        public StateVariable StateVariable { get; }
        public bool IsRetVal { get; }
        public Argument(string name, string direction, bool isRetVal, StateVariable stateVariable)
        {
            this.Name = name;
            this.Direction = direction;
            this.IsRetVal = isRetVal;
            this.StateVariable = stateVariable;
        }
    }
}