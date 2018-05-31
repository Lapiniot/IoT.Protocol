using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Soap;
using IoT.Protocol.Upnp;

namespace IoT.Protocol.Upnp.Services
{
    public class ConnectionManagerService : SoapActionInvoker
    {
        public ConnectionManagerService(SoapControlEndpoint endpoint, string deviceId) :
        base(endpoint, new Uri($"{deviceId}-MR/upnp.org-ConnectionManager-1/control", UriKind.Relative), UpnpServices.ConnectionManager)
        {
        }

        public Task<IDictionary<string, string>> GetProtocolInfoAsync(CancellationToken cancellationToken = default)
        {
            return InvokeAsync("GetProtocolInfo", cancellationToken);
        }
        
        public Task<IDictionary<string, string>> GetCurrentConnectionInfoAsync(int connectionId, CancellationToken cancellationToken = default)
        {
            return InvokeAsync("GetCurrentConnectionInfo", cancellationToken, ("ConnectionID", connectionId));
        }

        public Task<IDictionary<string, string>> GetCurrentConnectionIDsAsync(CancellationToken cancellationToken = default)
        {
            return InvokeAsync("GetCurrentConnectionIDs", cancellationToken);
        }
    }
}