namespace Cike.Core.Hashers;

/// <inheritdoc />
public class Hasher : IHasher, ISingletonDependency
{
    /// <inheritdoc />
    public string Hash(string value)
    {
        var data = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(data);
    }

    /// <inheritdoc />
    public string Hash(object?[] values, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        var strings = values.Select(e => Serialize(e, jsonSerializerOptions)).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        var input = string.Join("|", strings);
        return Hash(input);
    }

    public string Hash(params string?[] values)
    {
        var strings = values.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        var input = string.Join("|", strings);
        return Hash(input);
    }

    private string Serialize(object? payload, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        if (payload == null)
            return string.Empty;

        if (payload is string s)
            return s;

        return JsonSerializer.Serialize(payload, payload.GetType(), jsonSerializerOptions);
    }
}
