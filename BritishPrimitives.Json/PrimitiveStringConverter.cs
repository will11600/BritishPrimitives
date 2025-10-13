using System.Text.Json;

namespace BritishPrimitives.Json;

/// <summary>
/// Converts a <typeparamref name="TSource"/> to or from a JSON string.
/// </summary>
/// <typeparam name="TSource">The type to convert, which must be a struct and implement <see cref="IPrimitive{T}"/>.</typeparam>
public sealed class PrimitiveStringConverter<TSource> : PrimitiveConverter<TSource> where TSource : struct, IPrimitive<TSource>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrimitiveStringConverter{T}"/> class with specified format and format provider.
    /// </summary>
    /// <param name="format">The format string to use when writing the value.</param>
    /// <param name="formatProvider">The format provider to use when reading and writing the value.</param>
    public PrimitiveStringConverter(string? format, IFormatProvider? formatProvider = default)
    {
        Format = format;
        FormatProvider = formatProvider;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrimitiveStringConverter{T}"/> class with default settings.
    /// </summary>
    public PrimitiveStringConverter()
    {
        Format = default;
        FormatProvider = default;
    }

    /// <inheritdoc/>
    public override TSource Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return default;
        }

        try
        {
            return Parse<TSource>(in reader, TSource.MaxLength);
        }
        catch (FormatException ex)
        {
            throw new JsonException($"Failed to parse the primitive type '{typeof(TSource).Name}'.", ex);
        }
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, TSource value, JsonSerializerOptions options)
    {
        Span<char> buffer = stackalloc char[TSource.MaxLength];
        if (value.TryFormat(buffer, out int charsWritten, Format, FormatProvider))
        {
            writer.WriteStringValue(buffer[..charsWritten]);
            return;
        }
        
        throw new JsonException($"Failed to format the primitive type '{typeof(TSource).Name}'.");
    }
}
