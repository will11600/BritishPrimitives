using System.Collections.Concurrent;
using System.Reflection;

namespace BritishPrimitives.BitPacking;

internal static class EnumBitPacker
{
    private static readonly ConcurrentDictionary<Type, int> _enumSizes = [];

    public static bool TryPackEnum<T>(this ref readonly BitWriter writer, ref int position, T value) where T : struct, Enum
    {
        int max = _enumSizes.GetOrAdd(typeof(T), CalculateEnumSize<T>);
        return writer.TryPackInteger(ref position, Convert.ToUInt64(value), (ulong)max);
    }

    public static bool TryUnpackEnum<T>(this ref readonly BitReader reader, ref int position, out T result) where T : struct, Enum
    {
        int max = _enumSizes.GetOrAdd(typeof(T), CalculateEnumSize<T>);
        if (reader.TryUnpackInteger(ref position, out ulong resultValue, (ulong)max))
        {
            result = (T)Enum.ToObject(typeof(T), resultValue);
            return true;
        }

        return Helpers.FalseOutDefault(out result);
    }

    private static int CalculateEnumSize<T>(Type type) where T : struct, Enum
    {
        var values = Enum.GetValues<T>();

        if (type.GetCustomAttribute<FlagsAttribute>() is null)
        {
            return values.Length > 0 ? Convert.ToInt32(values.Max()) + 1 : 0;
        }

        int max = default;
        foreach (var value in values)
        {
            max |= Convert.ToInt32(value);
        }

        return max;
    }
}