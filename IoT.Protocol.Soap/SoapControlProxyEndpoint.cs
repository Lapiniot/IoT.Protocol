using System;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Soap
{
    public class SoapControlProxyEndpoint : IControlEndpoint<SoapEnvelope, SoapEnvelope>
    {
        private readonly SoapControlEndpoint endpoint;
        private readonly Uri relativeUri;

        public SoapControlProxyEndpoint(SoapControlEndpoint endpoint, Uri relativeUri)
        {
            this.endpoint = endpoint;
            this.relativeUri = relativeUri;
        }

        #region Implementation of IControlEndpoint<in SoapEnvelope,SoapEnvelope>

        public Task<SoapEnvelope> InvokeAsync(SoapEnvelope message, CancellationToken cancellationToken)
        {
            return endpoint.InvokeAsync(relativeUri, message, cancellationToken);
        }

        #endregion
    }
}