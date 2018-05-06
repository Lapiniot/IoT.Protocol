using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Soap;

namespace IoT.Protocol.Upnp
{
    public class UpnpControlEndpoint : SoapControlEndpoint
    {
        public string ServiceType { get; }
        public UpnpControlEndpoint(Uri baseAddress, string serviceType) : base(baseAddress)
        {
            this.ServiceType = serviceType;
        }

        public Task<SoapEnvelope> InvokeAsync(string action, (string Name, object Value)[] args, CancellationToken cancellationToken = default)
        {
            return InvokeAsync(new SoapEnvelope(action, ServiceType, args.ToDictionary(a => a.Name, a => a.Value)));
        }
    }
}