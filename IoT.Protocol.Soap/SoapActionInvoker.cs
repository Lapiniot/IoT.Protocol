using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IoT.Protocol.Soap
{
    public class SoapActionInvoker
    {
        public SoapActionInvoker(SoapControlEndpoint endpoint, Uri controlUri, string schema)
        {
            Target = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            Schema = schema ?? throw new ArgumentNullException(nameof(schema));

            if(controlUri?.IsAbsoluteUri == true) throw new ArgumentException("Must be valid uri relative to the base endpoint uri", nameof(controlUri));

            ControlUri = controlUri;
        }

        public SoapActionInvoker(SoapControlEndpoint endpoint, string schema) : this(endpoint, null, schema)
        {
        }

        public Uri ControlUri { get; }

        public SoapControlEndpoint Target { get; }

        public string Schema { get; }

        public async Task<IDictionary<string, string>> InvokeAsync(string action, IDictionary<string, string> args, CancellationToken cancellationToken = default)
        {
            return (await Target.InvokeAsync(ControlUri, new SoapEnvelope(action, Schema, args), cancellationToken).ConfigureAwait(false)).Arguments;
        }

        public async Task<IDictionary<string, string>> InvokeAsync(string action, CancellationToken cancellationToken = default, params (string, object)[] args)
        {
            return (await Target.InvokeAsync(ControlUri, new SoapEnvelope(action, Schema, args), cancellationToken).ConfigureAwait(false)).Arguments;
        }
    }
}