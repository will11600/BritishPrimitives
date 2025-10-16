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
        return writer.TryPackInteger(ref position, Convert<T, ulong>(ref value), max);
    }

    public static bool TryUnpackEnum<T>(this ref readonly BitReader reader, ref int position, out T result) where T : unmanaged, Enum
    {
        ulong max = _enumSizes.GetOrAdd(typeof(T), CalculateEnumSize<T>);
        if (reader.TryUnpackInteger(ref position, out ulong resultValue, max))
        {
            result = Convert<ulong, T>(ref resultValue);
            return true;
        }

        return Helpers.FalseOutDefault(out result);
    }

    private static ulong CalculateEnumSize<T>(Type type) where T : unmanaged, Enum
    {
        var values = Enum.GetValues<T>();
        return type.GetCustomAttribute<FlagsAttribute>() is null ? Max(values) : AggregateBitwiseOr(values);
    }

    private static ulong AggregateBitwiseOr<T>(T[] values) where T : unmanaged, Enum
    {
        ulong max = default;

        for (int i = 0; i < values.Length; i++)
        {
            max |= Convert<T, ulong>(ref values[i]);
        }

        return max;
    }

    private static ulong Max<T>(T[] values) where T : unmanaged, Enum
    {
        T max = values.Max();
        return values.Length > 0 ? Convert<T, ulong>(ref max) + 1UL : 0UL;
    }

    private static unsafe TResult Convert<TValue, TResult>(ref TValue value) where TValue : unmanaged where TResult : unmanaged
    {
        TResult result = default;

        byte* sourcePtr = (byte*)Unsafe.AsPointer(ref value);
        byte* destinationPtr = (byte*)&result;

        uint byteCount = (uint)Math.Min(sizeof(TValue), sizeof(TResult));
        Unsafe.CopyBlock(destinationPtr, sourcePtr, byteCount);

        return result;
    }
}