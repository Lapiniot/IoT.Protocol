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

        public async Task<IDictionary<string, string>> InvokeAsync(string action, IDictionary<string, string> args, CancellationToken cancellationToken = default)
        {
            return (await target.InvokeAsync(relativeUri, new SoapEnvelope(action, Schema, args), cancellationToken).ConfigureAwait(false)).Arguments;
        }

        public async Task<IDictionary<string, string>> InvokeAsync(string action, CancellationToken cancellationToken = default, params (string, object)[] args)
        {
            return (await target.InvokeAsync(relativeUri, new SoapEnvelope(action, Schema, args), cancellationToken).ConfigureAwait(false)).Arguments;
        }
    }
}