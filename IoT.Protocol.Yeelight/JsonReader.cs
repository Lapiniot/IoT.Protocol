using static System.Text.Json.JsonTokenType;

namespace IoT.Protocol.Yeelight;

public static class JsonReader
{
    private static readonly byte[] ResultPropName = { (byte)'r', (byte)'e', (byte)'s', (byte)'u', (byte)'l', (byte)'t' };
    private static readonly byte[] ErrorPropName = { (byte)'e', (byte)'r', (byte)'r', (byte)'o', (byte)'r' };
    private static readonly byte[] CodePropName = { (byte)'c', (byte)'o', (byte)'d', (byte)'e' };
    private static readonly byte[] MessagePropName = { (byte)'m', (byte)'e', (byte)'s', (byte)'s', (byte)'a', (byte)'g', (byte)'e' };

    public static bool TryReadResult(ref Utf8JsonReader reader, out JsonElement result, out int errorCode, out string errorMessage)
    {
        errorCode = 0;
        errorMessage = null;
        result = default;

        while (reader.CurrentDepth >= 1 && reader.Read())
        {
            if (reader is not { TokenType: PropertyName, CurrentDepth: 1 })
            {
                continue;
            }

            if (reader.ValueSpan.SequenceEqual(ResultPropName))
            {
                if (!reader.Read()) continue;
                result = JsonElement.ParseValue(ref reader);
                return true;
            }

            if (!reader.ValueSpan.SequenceEqual(ErrorPropName) || !reader.Read()) continue;

            var leftToRead = 2;
            while (reader.CurrentDepth >= 1 && leftToRead > 0 && reader.Read())
            {
                if (reader is not { TokenType: PropertyName, CurrentDepth: 2 })
                {
                    continue;
                }

                if (reader.ValueSpan.SequenceEqual(CodePropName))
                {
                    if (!reader.Read() || reader.TokenType is not Number) continue;
                    errorCode = reader.GetInt32();
                    leftToRead--;
                }
                else if (reader.ValueSpan.SequenceEqual(MessagePropName)
                         && reader.Read() && reader.TokenType is JsonTokenType.String)
                {
                    errorMessage = reader.GetString();
                    leftToRead--;
                }
            }

            return true;
        }

        return false;
    }
}