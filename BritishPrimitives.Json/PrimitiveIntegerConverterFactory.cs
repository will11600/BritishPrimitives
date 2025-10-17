using System.Text.Json.Serialization;

namespace BritishPrimitives.Json;

/// <summary>
/// Creates a <see cref="JsonConverter"/> for primitive types that are backed by an integer value.
/// </summary>
public sealed class PrimitiveIntegerConverterFactory : PrimitiveConverterFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrimitiveIntegerConverterFactory"/> class with specified format and format provider.
    /// </summary>
    /// <param name="format">The format string to use when writing the value.</param>
    /// <param name="formatProvider">The format provider to use when reading and writing the value.</param>
    public PrimitiveIntegerConverterFactory(string? format, IFormatProvider? formatProvider = default)
    {
        Format = format;
        FormatProvider = formatProvider;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrimitiveIntegerConverterFactory"/> class with default settings.
    /// </summary>
    public PrimitiveIntegerConverterFactory()
    {
        Format = default;
        FormatProvider = default;
    }

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        return base.CanConvert(typeToConvert) && typeToConvert.IsAssignableTo(typeof(ICastable<,>));
    }

    /// <inheritdoc/>
    protected override Type? ConverterOf(Type typeToConvert)
    {
        if (typeToConvert.GetInterface(typeof(ICastable<,>).Name) is Type castableInterface)
        {
            return typeof(PrimitiveIntegerConverter<,>).MakeGenericType(castableInterface.GetGenericArguments());
        }

        return default;
    }
}