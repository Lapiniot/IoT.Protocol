using System;

namespace IoT.Protocol.Upnp.Services
{
    public class ServiceSchemaAttribute : Attribute
    {
        public ServiceSchemaAttribute(string schema)
        {
            Schema = schema;
        }

        public string Schema { get; }
    }
}