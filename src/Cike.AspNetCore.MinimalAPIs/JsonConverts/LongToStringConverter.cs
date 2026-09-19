namespace Cike.AspNetCore.MinimalAPIs.JsonConverts;

public class LongToStringConverter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            if (long.TryParse(str, out var result))
                return result;
            throw new JsonException($"The value '{str}' is not a valid Int64.");
        }
        return reader.GetInt64();
    }

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

public class NullableLongToStringConverter : JsonConverter<long?>
{
    public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var str = reader.GetString();
            if (long.TryParse(str, out var result))
                return result;
            throw new JsonException($"The value '{str}' is not a valid Int64.");
        }
        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt64();
        }
        return null;
    }

    public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value?.ToString() ?? "0");
    }
}