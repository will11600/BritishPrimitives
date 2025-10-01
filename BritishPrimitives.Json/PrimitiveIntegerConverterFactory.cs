using System.Text.Json.Serialization;

namespace BritishPrimitives.Json;

/// <summary>
/// Creates a <see cref="JsonConverter"/> for primitive types that are backed by an integer value.
/// </summary>
public sealed class PrimitiveIntegerConverterFactory : PrimitiveConverterFactory
{
    /// <inheritdoc/>
    protected override JsonConverter? CreateInstance(Type converterType)
    {
        return Activator.CreateInstance(converterType) as JsonConverter;
    }
}