using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BritishPrimitives.Json;

/// <summary>
/// Converts a <typeparamref name="T"/> to or from a JSON integer.
/// </summary>
/// <typeparam name="T">The type to convert, which must be a struct and implement <see cref="IPrimitive{T}"/>.</typeparam>
/// <remarks>
/// This converter supports reading integers from both JSON numbers and JSON strings,
/// provided that <see cref="JsonNumberHandling.AllowReadingFromString"/> is set in <see cref="JsonSerializerOptions.NumberHandling"/>.
/// Writing supports both JSON number and JSON string formats based on <see cref="JsonNumberHandling.WriteAsString"/>.
/// </remarks>
public sealed class PrimitiveIntegerConverter<T> : JsonConverter<T> where T : struct, IPrimitive<T>
{
    private const int MaxUlongChars = 20;

    /// <summary>
    /// Reads and converts the JSON representation of the <typeparamref name="T"/>.
    /// </summary>
    /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
    /// <param name="typeToConvert">The type of object to convert.</param>
    /// <param name="options">An object that specifies serialization options.</param>
    /// <returns>The converted primitive integer value of type <typeparamref name="T"/>.</returns>
    /// <exception cref="JsonException">
    /// Thrown when the JSON token type is not <see cref="JsonTokenType.Null"/>, <see cref="JsonTokenType.String"/>, or <see cref="JsonTokenType.Number"/>,
    /// or if a string value cannot be successfully parsed as an unsigned 64-bit integer,
    /// or if <see cref="JsonNumberHandling.AllowReadingFromString"/> is not enabled and the token type is <see cref="JsonTokenType.String"/>.
    /// </exception>
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => default,
            JsonTokenType.String => ParseNumberFromString(ref reader, options),
            JsonTokenType.Number => (T)reader.GetUInt64(),
            _ => throw new JsonException(
                $"The JSON token type is not a valid number, string, or null for type '{nameof(T)}'. Received token type: {Enum.GetName(reader.TokenType)}.")
        };
    }

    /// <summary>
    /// Writes a integer value of type <typeparamref name="T"/> as JSON.
    /// </summary>
    /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
    /// <param name="value">The value to convert to JSON.</param>
    /// <param name="options">An object that specifies serialization options.</param>
    /// <exception cref="JsonException">
    /// Thrown if the primitive integer value cannot be formatted into a string when <see cref="JsonNumberHandling.WriteAsString"/> is enabled.
    /// </exception>
    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        ulong number = (ulong)value;

        if ((options.NumberHandling & JsonNumberHandling.WriteAsString) == 0)
        {
            writer.WriteNumberValue(number);
            return;
        }

        Span<char> chars = stackalloc char[MaxUlongChars];
        if (number.TryFormat(chars, out int charsWritten))
        {
            writer.WriteStringValue(chars[..charsWritten]);
            return;
        }

        throw new JsonException("Failed to format the number value to a string for serialization.");
    }

    private static T ParseNumberFromString(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if ((options.NumberHandling & JsonNumberHandling.AllowReadingFromString) == 0)
        {
            throw new JsonException($"Reading numbers from strings is disabled. Set '{nameof(JsonNumberHandling.AllowReadingFromString)}' to enable.");
        }

        if (ulong.TryParse(reader.GetString(), CultureInfo.InvariantCulture, out ulong number))
        {
            return (T)number;
        }

        throw new JsonException($"The JSON string value could not be parsed as an unsigned 64-bit integer for type '{nameof(T)}'.");
    }
}