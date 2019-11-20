using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace IoT.Protocol.Yeelight
{
    public struct RequestMessage
    {
        public string Method { get; set; }
        public string Sid { get; set; }
        public object Params { get; set; }

        public RequestMessage(string method)
        {
            Method = method;
            Params = null;
            Sid = null;
        }

        public RequestMessage(string method, object @params)
        {
            Method = method;
            Params = @params;
            Sid = null;
        }

        public RequestMessage(string method, object @params, string sid)
        {
            Method = method;
            Params = @params;
            Sid = sid;
        }

        public long SerializeTo(byte[] buffer, long id)
        {
            using var memoryStream = new MemoryStream(buffer);
            using var writer = new Utf8JsonWriter(memoryStream);

            writer.WriteStartObject();
            writer.WriteNumber("id", id);
            writer.WriteString("method", Method);
            if(!string.IsNullOrEmpty(Sid)) writer.WriteString("sid", Sid);
            if(Params == null)
            {
                writer.WriteStartArray("params");
                writer.WriteEndArray();
            }
            else
            {
                WriteObject(writer, "params", Params);
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
            switch(value)
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
                    foreach(var o in array) WriteValue(writer, o);
                    writer.WriteEndArray();
                    break;
                case IDictionary<string, object> dictionary:
                    writer.WriteStartObject();
                    foreach(var (k, v) in dictionary) WriteObject(writer, k, v);
                    writer.WriteEndObject();
                    break;
                case null:
                    writer.WriteNullValue();
                    break;
                default: throw new NotSupportedException("Not supported value type");
            }
        }
    }
}