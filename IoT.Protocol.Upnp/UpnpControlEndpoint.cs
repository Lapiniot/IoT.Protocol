using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Soap;

namespace IoT.Protocol.Upnp
{
    public class UpnpControlEndpoint : SoapControlEndpoint
    {
        public UpnpControlEndpoint(string serviceType, HttpClient httpClient) : base(httpClient)
        {
            ServiceType = serviceType;
        }

        public string ServiceType { get; }

        public async Task<IReadOnlyDictionary<string, string>> InvokeAsync(string action, IReadOnlyDictionary<string, string> args, CancellationToken cancellationToken = default)
        {
            return (await InvokeAsync(new SoapEnvelope(action, ServiceType, args), cancellationToken).ConfigureAwait(false)).Arguments;
        }
    }
}