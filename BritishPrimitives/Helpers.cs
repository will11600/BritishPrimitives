using BritishPrimitives.BitPacking;
using System.Runtime.CompilerServices;

namespace BritishPrimitives;

internal static class Helpers
{
    public const string FormatExceptionMessage = "Input string was not in a correct format.";

    public const int BitsPerByte = 8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool FalseOutDefault<T>(out T value) where T : struct
    {
        value = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendWhitespace(Span<char> chars, ref int charsWritten)
    {
        chars[charsWritten++] = Character.Whitespace;
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
    public static bool HasAvailableBits<T>(ref readonly T packer, int position, int bitLength) where T : struct, IBitPacker, allows ref struct
    {
        return (position + bitLength) <= packer.BitLength;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ClampAvailableBits<T>(ref readonly T packer, int position, int bitsPerItem, int count) where T : struct, IBitPacker, allows ref struct
    {
        return Math.Min((packer.BitLength - position) / bitsPerItem, count);
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
