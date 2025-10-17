using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BritishPrimitives.Json;

/// <summary>
/// Provides a base class for a custom <see cref="JsonConverter{T}"/> that handles
/// serialization and deserialization of primitive wrapper types.
/// </summary>
/// <typeparam name="TSource">The type of the primitive wrapper to convert, which must be a struct 
/// and implement <see cref="IPrimitive{TSource}"/>.</typeparam>
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

    /// <summary>
    /// Parses a primitive value from the JSON reader's current value span.
    /// </summary>
    /// <typeparam name="T">The type to parse the value into, which must implement <see cref="ISpanParsable{T}"/>.</typeparam>
    /// <param name="reader">The reader to read the value from. The reader must be positioned on a string value.</param>
    /// <param name="maxLength">The maximum length of the string value to read.</param>
    /// <returns>The parsed primitive value of type <typeparamref name="T"/>.</returns>
    /// <exception cref="JsonException">Thrown if the value cannot be parsed into type <typeparamref name="T"/>.</exception>
    protected T Parse<T>(ref readonly Utf8JsonReader reader, int maxLength) where T : ISpanParsable<T>
    {
        int charsWritten = 0;
        Span<char> chars = stackalloc char[maxLength];
        var decoder = Encoding.UTF8.GetDecoder();

        if (reader.HasValueSequence)
        {
            var enumerator = reader.ValueSequence.GetEnumerator();
            while (charsWritten < maxLength && enumerator.MoveNext())
            {
                var source = enumerator.Current.Span;
                var destination = chars[charsWritten..];
                decoder.Convert(source, destination, decoder.GetCharCount(source, false) < destination.Length, out _, out int charsUsed, out _);
                charsWritten += charsUsed;
            }
        }
        else
        {
            decoder.Convert(reader.ValueSpan, chars, true, out _, out charsWritten, out _);
        }
        
        return T.Parse(chars[..charsWritten], FormatProvider);
    }
}
