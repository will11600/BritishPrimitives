using BritishPrimitives.BitPacking;
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
    public static bool CanWrite(this ref readonly BitWriter writer, int position, int bitLength)
    {
        return position >= 0 && (position + bitLength) < writer.BitLength;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CanRead(this ref readonly BitReader writer, int position, int bitLength)
    {
        return position >= 0 && (position + bitLength) < writer.BitLength;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool SequenceEquals(byte* a, byte* b, int length)
    {
        ReadOnlySpan<byte> aSpan = new(a, length);
        ReadOnlySpan<byte> bSpan = new(b, length);
        return aSpan.SequenceEqual(bSpan);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ulong ConcatenateBytes(byte* ptr, int length)
    {
        ulong result = default;
        for (int i = 0; i < length; i++)
        {
            result |= (ulong)ptr[i] << (BitsPerByte * i);
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void SpreadBytes(ulong number, byte* ptr, int length)
    {
        for (int i = 0; i < length; i++)
        {
            ptr[i] = (byte)(number >> (BitsPerByte * i));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int BuildHashCode(byte* ptr, int length)
    {
        HashCode hashCode = new();
        for (int i = 0; i < length; i++)
        {
            hashCode.Add(ptr[i]);
        }
        return hashCode.ToHashCode();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CalculateOffset<T>(ReadOnlySpan<T> source, Range range)
    {
        var (offset, length) = range.GetOffsetAndLength(source.Length);
        return offset + length;
    }
}
