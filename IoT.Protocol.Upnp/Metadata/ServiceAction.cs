using System.Collections.Generic;

namespace IoT.Protocol.Upnp.Metadata
{
    public class ServiceAction
    {
        public ServiceAction(string name, IEnumerable<Argument> arguments)
        {
            Arguments = arguments;
            Name = name;
        }

        public string Name { get; }
        public IEnumerable<Argument> Arguments { get; }
    }
}