using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
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