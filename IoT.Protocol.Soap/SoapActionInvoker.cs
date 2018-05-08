using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Soap
{
    public class SoapActionInvoker
    {
        private readonly Uri relativeUri;
        private readonly SoapControlEndpoint target;

        public SoapActionInvoker(SoapControlEndpoint endpoint, string path, string schema)
        {
            target = endpoint;
            relativeUri = new Uri(path, UriKind.Relative);
            Schema = schema;
        }

        public string Schema { get; }

        public Task<SoapEnvelope> InvokeAsync(string action, IDictionary<string, object> args, CancellationToken cancellationToken)
        {
            return target.InvokeAsync(relativeUri, new SoapEnvelope(action, Schema, args), cancellationToken);
        }
    }
}