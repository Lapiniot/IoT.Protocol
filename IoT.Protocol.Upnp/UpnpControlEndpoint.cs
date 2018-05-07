using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Soap;

namespace IoT.Protocol.Upnp
{
    public class UpnpControlEndpoint : SoapControlEndpoint
    {
        public UpnpControlEndpoint(Uri baseAddress, string serviceType) : base(baseAddress)
        {
            ServiceType = serviceType;
        }

        public string ServiceType { get; }

        public Task<SoapEnvelope> InvokeAsync(string action, (string Name, object Value)[] args, CancellationToken cancellationToken = default)
        {
            return InvokeAsync(new SoapEnvelope(action, ServiceType, args.ToDictionary(a => a.Name, a => a.Value)), cancellationToken);
        }
    }
}