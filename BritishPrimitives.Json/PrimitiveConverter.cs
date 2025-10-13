using System.Text.Json.Serialization;

namespace BritishPrimitives.Json;

public abstract class PrimitiveConverter<TSource> : JsonConverter<TSource> where TSource : struct, IPrimitive<TSource>
{
    /// <summary>
    /// Gets the format string used for serialization, if specified.
    /// </summary>
    public string? Format { get; init; }

    /// <summary>
    /// Gets the format provider (culture) used for serialization and deserialization, if specified.
    /// </summary>
    public IFormatProvider? FormatProvider { get; init; }
}