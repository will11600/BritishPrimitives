using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BritishPrimitives.Json;

/// <summary>
/// Provides a base class for <see cref="JsonConverterFactory"/> implementations
/// that create converters for types implementing the <see cref="IPrimitive{T}"/> interface.
/// </summary>
public abstract class PrimitiveConverterFactory : JsonConverterFactory
{
    private static readonly ConcurrentDictionary<Type, JsonConverter?> _converters = [];

    private readonly object?[] _constructorParameters = new object?[2];

    /// <summary>
    /// Gets the format string used for serialization, if specified.
    /// </summary>
    public string? Format
    {
        get => _constructorParameters[0] as string;
        init => _constructorParameters[0] = value;
    }

    /// <summary>
    /// Gets the format provider (culture) used for serialization and deserialization, if specified.
    /// </summary>
    public IFormatProvider? FormatProvider
    {
        get => _constructorParameters[1] as IFormatProvider;
        init => _constructorParameters[1] = value;
    }

    /// <inheritdoc/>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsAssignableTo(typeof(IPrimitive<>));
    }

    /// <inheritdoc/>
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return _converters.GetOrAdd(typeToConvert, ConverterFactory);
    }

    /// <summary>
    /// Creates a new instance of <paramref name="typeToConvert"/>.
    /// </summary>
    /// <param name="typeToConvert">The <see cref="Type"/> being converted.</param>
    /// <returns>
    /// An instance of a <see cref="JsonConverter{T}"/> where T is compatible with <paramref name="typeToConvert"/>.
    /// If <see langword="null"/> is returned, a <see cref="NotSupportedException"/> will be thrown.
    /// </returns>
    protected JsonConverter? ConverterFactory(Type typeToConvert)
    {
        if (ConverterOf(typeToConvert) is Type factoryType)
        {
            return Activator.CreateInstance(factoryType, _constructorParameters) as JsonConverter;
        }

        return default;
    }

    /// <summary>
    /// When overridden in a derived class, returns the type of the converter to be created for the given type.
    /// </summary>
    /// <param name="typeToConvert">The type for which a converter is needed.</param>
    /// <returns>
    /// The <see cref="Type"/> of the specific <see cref="JsonConverter"/> to instantiate, 
    /// or <see langword="null"/> if no converter is available for the type.
    /// </returns>
    protected abstract Type? ConverterOf(Type typeToConvert);
}
