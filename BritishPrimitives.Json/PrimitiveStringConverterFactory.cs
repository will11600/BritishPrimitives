using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BritishPrimitives.Json;

/// <summary>
/// Creates a <see cref="JsonConverter"/> for primitive types that can be serialized as strings,
/// allowing optional format and format provider settings.
/// </summary>
public sealed class PrimitiveStringConverterFactory : PrimitiveConverterFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrimitiveStringConverterFactory"/> class with specified format and format provider.
    /// </summary>
    /// <param name="format">The format string to use when writing the value.</param>
    /// <param name="formatProvider">The format provider to use when reading and writing the value.</param>
    public PrimitiveStringConverterFactory(string? format, IFormatProvider? formatProvider = default)
    {
        Format = format;
        FormatProvider = formatProvider;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrimitiveStringConverterFactory"/> class with default settings.
    /// </summary>
    public PrimitiveStringConverterFactory()
    {
        Format = default;
        FormatProvider = default;
    }

    /// <inheritdoc/>
    protected override Type? ConverterOf(Type typeToConvert)
    {
        return typeof(PrimitiveStringConverter<>).MakeGenericType(typeToConvert);
    }
}
