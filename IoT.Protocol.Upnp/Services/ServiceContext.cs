using System;
using IoT.Protocol.Soap;

namespace IoT.Protocol.Upnp.Services
{
    public abstract class ServiceContext : SoapActionInvoker
    {
        protected ServiceContext(SoapControlEndpoint endpoint, string schema) : base(endpoint, new Uri(".", UriKind.Relative), schema)
        {
        }

        protected ServiceContext(SoapControlEndpoint endpoint, Uri controlUri, string schema) : base(endpoint, controlUri, schema)
        {
        }
    }
}