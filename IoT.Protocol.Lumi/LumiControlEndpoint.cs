using System;
using System.Json;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using IoT.Protocol.Net.Udp;

namespace IoT.Protocol.Lumi
{
    public class LumiControlEndpoint : UdpDispatchingEndpoint<JsonObject, JsonObject, string>
    {
        public LumiControlEndpoint(IPEndPoint endpoint) : base(endpoint)
        {
        }

        protected override TimeSpan CommandTimeout { get; } = TimeSpan.FromSeconds(10);

        protected override bool TryParseResponse(IPEndPoint remoteEndPoint, byte[] datagram, out string id, out JsonObject response)
        {
            var json = JsonExtensions.Deserialize(datagram);

            if(json is JsonObject jObject && jObject.TryGetValue("cmd", out var cmd) && jObject.TryGetValue("sid", out var sid))
            {
                id = GetCommandKey(GetCmdName(cmd), sid);
                response = jObject;
                return true;
            }

            id = null;
            response = null;
            return false;
        }

        protected override Task<(string, byte[])> CreateRequestAsync(JsonObject message, CancellationToken cancellationToken)
        {
            return Task.FromResult((GetCommandKey(message["cmd"], message["sid"]), message.Serialize()));
        }

        private string GetCommandKey(string command, string sid)
        {
            return $"{command}.{sid}";
        }

        private string GetCmdName(string command)
        {
            return command.EndsWith("_ack") ? command.Substring(0, command.Length - 4) : command;
        }

        public Task<JsonObject> InvokeAsync(string command, string sid, CancellationToken cancellationToken = default)
        {
            return InvokeAsync(new JsonObject {{"cmd", command}, {"sid", sid}}, cancellationToken);
        }
    }
}