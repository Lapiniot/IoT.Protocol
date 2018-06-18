using System;
using IoT.Protocol.Soap;

namespace IoT.Protocol.Upnp.Services
{
    public sealed class RenderingControlService : SoapActionInvoker
    {
        public RenderingControlService(SoapControlEndpoint endpoint, string deviceId) :
            base(endpoint, new Uri($"{deviceId}-MR/upnp.org-RenderingControl-1/control", UriKind.Relative), UpnpServices.RenderingControl)
        {
        }
    }
}