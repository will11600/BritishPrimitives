using System.Text.Json;
using System.Text.Json.Serialization;

namespace BritishPrimitives.Json;

public sealed class BritishPrimitiveStringConverter<T> : JsonConverter<T> where T : struct, IPrimitive<T>
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
