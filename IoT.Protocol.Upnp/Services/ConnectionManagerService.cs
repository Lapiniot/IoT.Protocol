using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Soap;
using static IoT.Protocol.Upnp.UpnpServices;

namespace IoT.Protocol.Upnp.Services
{
    [ServiceSchema(ConnectionManager)]
    public class ConnectionManagerService : SoapActionInvoker
    {
        public ConnectionManagerService(SoapControlEndpoint endpoint, Uri controlUri) :
            base(endpoint, controlUri, ConnectionManager) {}

        public ConnectionManagerService(SoapControlEndpoint endpoint) :
            base(endpoint, ConnectionManager) {}

        public Task<IReadOnlyDictionary<string, string>> GetProtocolInfoAsync(CancellationToken cancellationToken = default)
        {
            return InvokeAsync("GetProtocolInfo", null, cancellationToken);
        }

        public Task<IReadOnlyDictionary<string, string>> GetCurrentConnectionInfoAsync(string connectionId, CancellationToken cancellationToken = default)
        {
            return InvokeAsync("GetCurrentConnectionInfo", new Dictionary<string, string>() {
                { "ConnectionID", connectionId } },
                cancellationToken);
        }

        public Task<IReadOnlyDictionary<string, string>> GetCurrentConnectionIDsAsync(CancellationToken cancellationToken = default)
        {
            return InvokeAsync("GetCurrentConnectionIDs", null, cancellationToken);
        }
    }
}