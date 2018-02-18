using System.Json;

namespace IoT.Protocol
{
    public interface IJsonControlEndpoint : IControlEndpoint<JsonObject, JsonValue>
    {
    }
}