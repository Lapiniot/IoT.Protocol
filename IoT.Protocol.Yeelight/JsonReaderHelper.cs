using static System.Text.Json.JsonTokenType;

namespace IoT.Protocol.Yeelight;

public static class JsonReaderHelper
{
    public static bool TryReadResult(ref Utf8JsonReader reader, out JsonElement result,
        out int errorCode, out string errorMessage)
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

            if (reader.ValueSpan.SequenceEqual("result"u8))
            {
                if (!reader.Read())
                {
                    continue;
                }

                result = JsonElement.ParseValue(ref reader);
                return true;
            }

            if (!reader.ValueSpan.SequenceEqual("error"u8) || !reader.Read())
            {
                continue;
            }

            var leftToRead = 2;
            while (reader.CurrentDepth >= 1 && leftToRead > 0 && reader.Read())
            {
                if (reader is not { TokenType: PropertyName, CurrentDepth: 2 })
                {
                    continue;
                }

                if (reader.ValueSpan.SequenceEqual("code"u8))
                {
                    if (!reader.Read() || reader.TokenType is not Number)
                    {
                        continue;
                    }

                    errorCode = reader.GetInt32();
                    leftToRead--;
                }
                else if (reader.ValueSpan.SequenceEqual("message"u8)
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