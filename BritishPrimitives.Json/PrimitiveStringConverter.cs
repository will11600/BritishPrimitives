using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BritishPrimitives.Json;

/// <summary>
/// Converts a <typeparamref name="T"/> to or from a JSON string.
/// </summary>
/// <typeparam name="T">The type to convert, which must be a struct and implement <see cref="IPrimitive{T}"/>.</typeparam>
public sealed class PrimitiveStringConverter<T> : JsonConverter<T> where T : struct, IPrimitive<T>
{
    /// <summary>
    /// Gets the format string used for serialization, if specified.
    /// </summary>
    public string? Format { get; init; }

    /// <summary>
    /// Gets the format provider (culture) used for serialization and deserialization, if specified.
    /// </summary>
    public IFormatProvider? FormatProvider { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrimitiveStringConverter{T}"/> class 
    /// with the specified format string and format provider.
    /// </summary>
    /// <param name="format">The format string to use when writing the value.</param>
    /// <param name="formatProvider">The format provider to use when reading and writing the value.</param>
    public PrimitiveStringConverter(string format, IFormatProvider formatProvider)
    {
        Format = format;
        FormatProvider = formatProvider;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrimitiveStringConverter{T}"/> class 
    /// using default formatting (null format and invariant culture).
    /// </summary>
    public PrimitiveStringConverter()
    {
        Format = default;
        FormatProvider = default;
    }

    /// <summary>
    /// Reads a primitive value of type <typeparamref name="T"/> from a JSON string.
    /// </summary>
    /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
    /// <param name="typeToConvert">The type of object to convert.</param>
    /// <param name="options">An object that specifies serialization options.</param>
    /// <returns>The deserialized value of type <typeparamref name="T"/>.</returns>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return default;
        }

        if (!reader.HasValueSequence)
        {
            Span<char> buffer = stackalloc char[T.MaxLength];
            if (Encoding.UTF8.TryGetChars(reader.ValueSpan, buffer, out int charsWritten))
            {
                return T.Parse(buffer[..charsWritten], FormatProvider);
            }
        }

        return T.Parse(reader.GetString(), FormatProvider);
    }

    /// <summary>
    /// Writes a primitive value of type <typeparamref name="T"/> as a JSON string.
    /// </summary>
    /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
    /// <param name="value">The value to convert to JSON.</param>
    /// <param name="options">An object that specifies serialization options.</param>
    /// <exception cref="JsonException">Thrown if the primitive type fails to format.</exception>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        Span<char> buffer = stackalloc char[T.MaxLength];
        if (value.TryFormat(buffer, out int charsWritten, Format, FormatProvider))
        {
            writer.WriteStringValue(buffer[..charsWritten]);
            return;
        }

        throw new JsonException($"Failed to format the primitive type '{typeof(T).Name}'.");
    }
}