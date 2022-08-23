using System.Text.Json;
using System.Text.Json.Serialization;

namespace IoT.Protocol.Lumi;

[JsonSerializable(typeof(JsonElement))]
[JsonSerializable(typeof(IDictionary<string, object>))]
internal sealed partial class JsonContext : JsonSerializerContext { }