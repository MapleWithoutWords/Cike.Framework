namespace Cike.Core.Extensions.System;

public static class JsonSerializerOptionsExtensions
{
    /// <summary>
    /// Adds the specified converters to the options.
    /// </summary>
    public static JsonSerializerOptions WithConverters(this JsonSerializerOptions options, params JsonConverter[] converters)
    {
        foreach (var converter in converters)
            options.Converters.Add(converter);

        return options;
    }

    /// <summary>
    /// Clones the options.
    /// </summary>
    public static JsonSerializerOptions Clone(this JsonSerializerOptions options)
    {
        return new(options);
    }
}