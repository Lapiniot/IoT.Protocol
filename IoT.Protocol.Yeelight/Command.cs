using System.Text.Json.Nodes;

namespace IoT.Protocol.Yeelight;

public readonly record struct Command(string Method, object Params)
{
    private readonly JsonElement paramsElement;

    public Command(string method) : this(method, (object)null) { }
    public Command(string method, Action<Utf8JsonWriter> paramsWriter) : this(method, (object)paramsWriter) { }
    public Command(string method, JsonNode @params) : this(method, (object)@params) { }
    public Command(string method, JsonElement @params) : this(method, (object)null) => paramsElement = @params;

    public long WriteTo(byte[] buffer, long id)
    {
        using var ms = new MemoryStream(buffer);
        using var writer = new Utf8JsonWriter(ms);

        writer.WriteStartObject();
        writer.WriteNumber("id", id);
        writer.WriteString("method", Method);

        if (paramsElement.ValueKind != JsonValueKind.Undefined)
        {
            writer.WritePropertyName("params");
            paramsElement.WriteTo(writer);
        }
        else
        {
            switch (Params)
            {
                case null:
                    writer.WriteStartArray("params");
                    writer.WriteEndArray();
                    break;
                case Action<Utf8JsonWriter> writeAction:
                    writer.WritePropertyName("params");
                    writeAction(writer);
                    break;
                case JsonNode jsonNode:
                    writer.WritePropertyName("params");
                    jsonNode.WriteTo(writer);
                    break;
                default:
                    WriteObject(writer, "params", Params);
                    break;
            }
        }

        writer.WriteEndObject();
        writer.Flush();

        return writer.BytesCommitted;
    }

    private static void WriteObject(Utf8JsonWriter writer, string name, object value)
    {
        writer.WritePropertyName(name);
        WriteValue(writer, value);
    }

    private static void WriteValue(Utf8JsonWriter writer, object value)
    {
        switch (value)
        {
            case string strValue:
                writer.WriteStringValue(strValue);
                break;
            case byte byteValue:
                writer.WriteNumberValue(byteValue);
                break;
            case int intValue:
                writer.WriteNumberValue(intValue);
                break;
            case long longValue:
                writer.WriteNumberValue(longValue);
                break;
            case uint uintValue:
                writer.WriteNumberValue(uintValue);
                break;
            case ulong ulongValue:
                writer.WriteNumberValue(ulongValue);
                break;
            case float floatValue:
                writer.WriteNumberValue(floatValue);
                break;
            case double doubleValue:
                writer.WriteNumberValue(doubleValue);
                break;
            case decimal decimalValue:
                writer.WriteNumberValue(decimalValue);
                break;
            case bool boolValue:
                writer.WriteBooleanValue(boolValue);
                break;
            case Array array:
                writer.WriteStartArray();
                foreach (var o in array)
                {
                    WriteValue(writer, o);
                }

                writer.WriteEndArray();
                break;
            case IDictionary<string, object> dictionary:
                writer.WriteStartObject();
                foreach (var (k, v) in dictionary)
                {
                    WriteObject(writer, k, v);
                }

                writer.WriteEndObject();
                break;
            case null:
                writer.WriteNullValue();
                break;
            default: throw new NotSupportedException("Not supported value type");
        }
    }
}