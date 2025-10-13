using System.Text.Json;

namespace BritishPrimitives.Json;

/// <summary>
/// Converts a <typeparamref name="TSource"/> to or from a JSON string.
/// </summary>
/// <typeparam name="TSource">The type to convert, which must be a struct and implement <see cref="IPrimitive{T}"/>.</typeparam>
public sealed class PrimitiveStringConverter<TSource> : PrimitiveConverter<TSource> where TSource : struct, IPrimitive<TSource>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrimitiveStringConverter{T}"/> class 
    /// with the specified format string and format provider.
    /// </summary>
    /// <param name="format">The format string to use when writing the value.</param>
    /// <param name="formatProvider">The format provider to use when reading and writing the value.</param>
    public PrimitiveStringConverter(string format, IFormatProvider formatProvider) : base()
    {
        Format = format;
        FormatProvider = formatProvider;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrimitiveStringConverter{T}"/> class 
    /// using default formatting (null format and invariant culture).
    /// </summary>
    public PrimitiveStringConverter() : base()
    {
        Format = default;
        FormatProvider = default;
    }

    /// <summary>
    /// Reads a primitive value of type <typeparamref name="TSource"/> from a JSON string.
    /// </summary>
    /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
    /// <param name="typeToConvert">The type of object to convert.</param>
    /// <param name="options">An object that specifies serialization options.</param>
    /// <returns>The deserialized value of type <typeparamref name="TSource"/>.</returns>
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

    /// <summary>
    /// Writes a primitive value of type <typeparamref name="TSource"/> as a JSON string.
    /// </summary>
    /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
    /// <param name="value">The value to convert to JSON.</param>
    /// <param name="options">An object that specifies serialization options.</param>
    /// <exception cref="JsonException">Thrown if the primitive type fails to format.</exception>
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
