using System.Collections.Concurrent;
using System.Reflection;

namespace BritishPrimitives.BitPacking;

internal static class EnumBitPacker
{
    private static readonly ConcurrentDictionary<Type, ulong> _enumSizes = [];

    public static bool TryPackEnum<T>(this ref readonly BitWriter writer, ref int position, T value) where T : struct, Enum
    {
        ulong max = _enumSizes.GetOrAdd(typeof(T), CalculateEnumSize);
        return writer.TryPackInteger(ref position, Convert.ToUInt64(value), max);
    }

    public static bool TryUnpackEnum<T>(this ref readonly BitReader reader, ref int position, out T result) where T : struct, Enum
    {
        ulong max = _enumSizes.GetOrAdd(typeof(T), CalculateEnumSize);
        if (reader.TryUnpackInteger(ref position, out ulong resultValue, max))
        {
            result = (T)Enum.ToObject(typeof(T), resultValue);
            return true;
        }

        return Helpers.FalseOutDefault(out result);
    }

    private static ulong CalculateEnumSize(Type type)
    {
        if (type.GetCustomAttribute<FlagsAttribute>() is null)
        {
            var values = Enum.GetValuesAsUnderlyingType(type).Cast<ulong>();
            return values.Any() ? values.Max() : 0;
        }

        ulong max = default;
        foreach (var value in Enum.GetValuesAsUnderlyingType(type))
        {
            max |= (ulong)value;
        }

        return max;
    }
}