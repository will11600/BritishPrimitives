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

    /// <summary>
    /// Determines whether the converter factory can convert the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to check.</param>
    /// <returns><see langword="true"/> if the type implements <see cref="IPrimitive{T}"/>; otherwise, <see langword="false"/>.</returns>
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsAssignableTo(typeof(IPrimitive<>));
    }

    /// <summary>
    /// Creates a converter for the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>A new <see cref="JsonConverter"/> instance, or <see langword="null"/> if the type cannot be converted.</returns>
    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type converter = typeof(PrimitiveStringConverter<>).MakeGenericType(typeToConvert);
        return _converters.GetOrAdd(converter, CreateInstance);
    }

    /// <summary>
    /// Creates a new instance of the specific <see cref="JsonConverter"/> type.
    /// </summary>
    /// <param name="converterType">The generic converter type specialized for the primitive type.</param>
    /// <returns>A new <see cref="JsonConverter"/> instance.</returns>
    protected abstract JsonConverter? CreateInstance(Type converterType);
}
