using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BritishPrimitives.BitPacking;

internal static class EnumBitPacker
{
    private static readonly ConcurrentDictionary<Type, ulong> _enumSizes = [];

    public static bool TryPackEnum<T>(this ref readonly BitWriter writer, ref int position, T value) where T : unmanaged, Enum
    {
        ulong max = _enumSizes.GetOrAdd(typeof(T), CalculateEnumSize<T>);
        return writer.TryPackInteger(ref position, Convert.ToUInt64(value), max);
    }

    public static unsafe bool TryUnpackEnum<T>(this ref readonly BitReader reader, ref int position, out T result) where T : unmanaged, Enum
    {
        ulong max = _enumSizes.GetOrAdd(typeof(T), CalculateEnumSize<T>);
        if (reader.TryUnpackInteger(ref position, out ulong resultValue, max))
        {
            result = Unsafe.As<ulong, T>(ref resultValue);
            return true;
        }

        return Helpers.FalseOutDefault(out result);
    }

    private static ulong CalculateEnumSize<T>(Type type) where T : unmanaged, Enum
    {
        var values = Enum.GetValues<T>();

        if (type.GetCustomAttribute<FlagsAttribute>() is null)
        {
            return values.Length > 0 ? Convert.ToUInt64(values.Max()) + 1UL : 0UL;
        }

        ulong max = default;
        foreach (var value in values)
        {
            max |= Convert.ToUInt64(value);
        }

        return max;
    }
}