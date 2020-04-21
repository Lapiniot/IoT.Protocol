using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IoT.Protocol.Upnp.DIDL
{
    public class ItemJsonConverter : JsonConverter<Item>
    {
        public override Item Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return JsonSerializer.Deserialize<Item>(ref reader, options);
        }

        public override void Write(Utf8JsonWriter writer, Item value, JsonSerializerOptions options)
        {
            switch(value)
            {
                case Container container:
                    JsonSerializer.Serialize(writer, container, options);
                    break;
                case MediaItem mediaItem:
                    JsonSerializer.Serialize(writer, mediaItem, options);
                    break;
                default:
                    throw new NotSupportedException("Unsupported type");
            }
        }
    }
}