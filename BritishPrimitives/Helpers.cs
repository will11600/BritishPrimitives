using BritishPrimitives.BitPacking;
using System.Buffers.Text;
using System.Runtime.CompilerServices;

namespace BritishPrimitives;

internal static class Helpers
{
    public const int BitsPerByte = 8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool FalseOutDefault<T>(out T value) where T : struct
    {
        value = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Pow(uint x, uint y)
    {
        ulong result = 1;
        ulong baseValue = x;

        while (y > 0)
        {
            if ((y & 1) == 1)
            {
                result *= baseValue;
            }

            baseValue *= baseValue;

            y >>= 1;
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Max(uint digitCount, uint baseN = 10U)
    {
        return Pow(digitCount, baseN) - 1L;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CanWrite(this ref readonly BitWriter writer, int position, int bitLength)
    {
        return position >= 0 && (position + bitLength) < writer.BitLength;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CanRead(this ref readonly BitReader writer, int position, int bitLength)
    {
        return position >= 0 && (position + bitLength) < writer.BitLength;
    }
}
