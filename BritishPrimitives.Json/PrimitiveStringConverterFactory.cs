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
    private readonly object?[] _constructorParams = new object[2];

    /// <summary>
    /// Gets the format string used for serialization, if specified.
    /// </summary>
    public string? Format
    {
        get => _constructorParams[0] as string;
        init => _constructorParams[0] = value;
    }

    /// <summary>
    /// Gets the format provider (culture) used for serialization and deserialization, if specified.
    /// </summary>
    public IFormatProvider? FormatProvider
    {
        get => _constructorParams[1] as IFormatProvider;
        init => _constructorParams[1] = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrimitiveStringConverterFactory"/> class with specified format and format provider.
    /// </summary>
    /// <param name="format">The format string to use when writing the value.</param>
    /// <param name="formatProvider">The format provider to use when reading and writing the value.</param>
    public PrimitiveStringConverterFactory(string format, IFormatProvider formatProvider)
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
    protected override JsonConverter? CreateInstance(Type converterType)
    {
        return Activator.CreateInstance(converterType, _constructorParams) as JsonConverter;
    }
}
