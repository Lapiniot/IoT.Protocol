namespace IoT.Protocol.Upnp.Metadata
{
    public class ServiceAction
    {
        public string Name { get; }
        public Argument[] Arguments { get; }
        public ServiceAction(string name, Argument[] arguments)
        {
            this.Arguments = arguments;
            this.Name = name;
        }
    }
}