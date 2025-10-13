using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BritishPrimitives.Json;

/// <summary>
/// A <see cref="JsonConverter{T}"/> that handles the serialization and deserialization
/// of primitive types <typeparamref name="TSource"/> that are internally represented by an unsigned integer type <typeparamref name="TProduct"/>.
/// </summary>
/// <remarks>
/// This converter is designed for primitive structs that implement <see cref="IPrimitive{TSelf}"/> and <see cref="ICastable{TSelf, TValue}"/>,
/// allowing them to be efficiently serialized as JSON numbers or strings based on their underlying unsigned integer value.
/// </remarks>
/// <typeparam name="TSource">
/// The primitive struct type, which must implement <see cref="IPrimitive{TSelf}"/> and <see cref="ICastable{TSelf, TValue}"/>
/// for conversion to and from <typeparamref name="TProduct"/>.
/// </typeparam>
/// <typeparam name="TProduct">
/// The underlying unsigned integer type (e.g., <see langword="byte"/>, <see langword="ushort"/>, <see langword="uint"/>, <see langword="ulong"/>)
/// that represents the compact form of the primitive.
/// </typeparam>
public sealed class PrimitiveIntegerConverter<TSource, TProduct> : PrimitiveConverter<TSource>
  where TSource : struct, IPrimitive<TSource>, ICastable<TSource, TProduct>
  where TProduct : unmanaged, IUnsignedNumber<TProduct>, IMinMaxValue<TProduct>
{
    private delegate TSource ReadDelegate(ref readonly Utf8JsonReader reader);
    private delegate void WriteDelegate(Utf8JsonWriter writer, ref readonly TProduct product);

    private static readonly ReadDelegate _read;
    private static readonly WriteDelegate _write;

    private static readonly Lazy<int> _maxCharacterLength;

    static unsafe PrimitiveIntegerConverter()
    {
        _maxCharacterLength = new Lazy<int>(CalculateMaxCharacterLength, LazyThreadSafetyMode.ExecutionAndPublication);
        switch (sizeof(TProduct))
        {
            case sizeof(byte):
                _read = static (ref readonly Utf8JsonReader r) => (TSource)Unsafe.BitCast<byte, TProduct>(r.GetByte());
                _write = static (Utf8JsonWriter w, ref readonly TProduct p) => w.WriteNumberValue(Unsafe.BitCast<TProduct, byte>(p));
                break;
            case sizeof(ushort):
                _read = static (ref readonly Utf8JsonReader r) => (TSource)Unsafe.BitCast<ushort, TProduct>(r.GetUInt16());
                _write = static (Utf8JsonWriter w, ref readonly TProduct p) => w.WriteNumberValue(Unsafe.BitCast<TProduct, ushort>(p));
                break;
            case sizeof(uint):
                _read = static (ref readonly Utf8JsonReader r) => (TSource)Unsafe.BitCast<uint, TProduct>(r.GetUInt32());
                _write = static (Utf8JsonWriter w, ref readonly TProduct p) => w.WriteNumberValue(Unsafe.BitCast<TProduct, uint>(p));
                break;
            case sizeof(ulong):
                _read = static (ref readonly Utf8JsonReader r) => (TSource)Unsafe.BitCast<ulong, TProduct>(r.GetUInt64());
                _write = static (Utf8JsonWriter w, ref readonly TProduct p) => w.WriteNumberValue(Unsafe.BitCast<TProduct, ulong>(p));
                break;
            default:
                throw new NotSupportedException($"The size of {typeof(TProduct).Name} is not supported for serialization.");
        };
    }

    /// <summary>
    /// Reads and converts the JSON representation of the <typeparamref name="TSource"/>.
    /// </summary>
    /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
    /// <param name="typeToConvert">The type of object to convert.</param>
    /// <param name="options">An object that specifies serialization options.</param>
    /// <returns>The converted primitive integer value of type <typeparamref name="TSource"/>.</returns>
    /// <exception cref="JsonException">
    /// Thrown when the JSON token type is not <see cref="JsonTokenType.Null"/>, <see cref="JsonTokenType.String"/>, or <see cref="JsonTokenType.Number"/>,
    /// or if a string value cannot be successfully parsed as an unsigned integer of type <typeparamref name="TProduct"/>,
    /// or if <see cref="JsonNumberHandling.AllowReadingFromString"/> is not enabled and the token type is <see cref="JsonTokenType.String"/>.
    /// </exception>
    public override TSource Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType switch
    {
        JsonTokenType.Null => default!,
        JsonTokenType.String => ParseNumberFromString(ref reader, options),
        JsonTokenType.Number => _read(in reader),
        _ => throw new JsonException(
          $"The JSON token type is not a valid number, string, or null for type '{typeof(TSource).Name}'. Received token type: {Enum.GetName(reader.TokenType)}.")
    };

    /// <summary>
    /// Writes the <typeparamref name="TSource"/> value as a JSON number or string.
    /// </summary>
    /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
    /// <param name="value">The primitive value of type <typeparamref name="TSource"/> to convert to JSON.</param>
    /// <param name="options">An object that specifies serialization options, including <see cref="JsonNumberHandling"/>.</param>
    /// <exception cref="JsonException">
    /// Thrown if the primitive integer value cannot be formatted into a string when <see cref="JsonNumberHandling.WriteAsString"/> is enabled.
    /// </exception>
    public override void Write(Utf8JsonWriter writer, TSource value, JsonSerializerOptions options)
    {
        TProduct number = (TProduct)value;

        if ((options.NumberHandling & JsonNumberHandling.WriteAsString) == 0)
        {
            _write(writer, in number);
            return;
        }

        Span<char> chars = stackalloc char[_maxCharacterLength.Value];
        if (number.TryFormat(chars, out int charsWritten, Format, FormatProvider))
        {
            writer.WriteStringValue(chars[..charsWritten]);
            return;
        }

        throw new JsonException("Failed to format the number value to a string for serialization.");
    }

    private TSource ParseNumberFromString(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        if ((options.NumberHandling & JsonNumberHandling.AllowReadingFromString) == 0)
        {
            throw new JsonException($"Reading numbers from strings is disabled. Set '{nameof(JsonNumberHandling.AllowReadingFromString)}' to enable.");
        }

        try
        {
            TProduct integer = Parse<TProduct>(in reader, _maxCharacterLength.Value);
            return (TSource)integer;
        }
        catch (FormatException ex)
        {
            throw new JsonException($"The JSON string value could not be parsed as a {typeof(TProduct).Name} for type '{typeof(TSource).Name}'.", ex);
        }
    }

    private static int CalculateMaxCharacterLength()
    {
        ulong max = ulong.CreateTruncating(TProduct.MaxValue);
        double log10Max = BigInteger.Log10(max);
        return (int)(log10Max + 0.5);
    }
}