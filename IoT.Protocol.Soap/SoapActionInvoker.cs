using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Soap
{
    public class SoapActionInvoker
    {
        private readonly SoapControlEndpoint target;
        private readonly Uri uri;

        public SoapActionInvoker(SoapControlEndpoint endpoint, Uri controlUri, string schema)
        {
            target = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            uri = controlUri ?? throw new ArgumentNullException(nameof(controlUri));
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));

            if(controlUri.IsAbsoluteUri) throw new ArgumentException("Must be valid uri relative to the base endpoint uri", nameof(controlUri));
        }

        public string Schema { get; }

        public async Task<IDictionary<string, string>> InvokeAsync(string action, IDictionary<string, string> args, CancellationToken cancellationToken = default)
        {
            return (await target.InvokeAsync(uri, new SoapEnvelope(action, Schema, args), cancellationToken).ConfigureAwait(false)).Arguments;
        }

        public async Task<IDictionary<string, string>> InvokeAsync(string action, CancellationToken cancellationToken = default, params (string, object)[] args)
        {
            return (await target.InvokeAsync(uri, new SoapEnvelope(action, Schema, args), cancellationToken).ConfigureAwait(false)).Arguments;
        }
    }
}