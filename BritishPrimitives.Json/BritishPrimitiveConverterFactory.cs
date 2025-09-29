using System.Text.Json;
using System.Text.Json.Serialization;

namespace BritishPrimitives.Json;

public sealed class BritishPrimitiveConverterFactory<TConverter, TPrimitive> : JsonConverterFactory
    where TConverter : JsonConverter<TPrimitive>
    where TPrimitive : struct, IPrimitive<TPrimitive>
{
    public override bool CanConvert(Type typeToConvert)
    {
        throw new NotImplementedException();
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
