using System;
using IoT.Protocol.Soap;
using static IoT.Protocol.Upnp.UpnpServices;

namespace IoT.Protocol.Upnp.Services
{
    [ServiceSchema(RenderingControl)]
    public sealed class RenderingControlService : SoapActionInvoker
    {
        public RenderingControlService(SoapControlEndpoint endpoint, Uri controlUri) :
            base(endpoint, controlUri, RenderingControl)
        {
        }

        public RenderingControlService(SoapControlEndpoint endpoint) :
            base(endpoint, RenderingControl)
        {
        }
    }
}