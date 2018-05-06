namespace IoT.Protocol.Upnp.Metadata
{
    public class ServiceAction
    {
        public ServiceAction(string name, Argument[] arguments)
        {
            Arguments = arguments;
            Name = name;
        }

        public string Name { get; }
        public Argument[] Arguments { get; }
    }
}