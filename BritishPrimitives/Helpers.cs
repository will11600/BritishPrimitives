using BritishPrimitives.BitPacking;
using BritishPrimitives.Text;
using System.Numerics;
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
    public static void Append(Span<char> destination, ReadOnlySpan<char> source, ref int charsWritten)
    {
        source.CopyTo(destination[charsWritten..]);
        charsWritten += source.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryAppend(Span<char> destination, ReadOnlySpan<char> source, ref int charsWritten, int buffer = 0)
    {
        if ((source.Length + charsWritten + buffer) > destination.Length)
        {
            return false;
        }

        Append(destination, source, ref charsWritten);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Append(Span<char> destination, char character, ref int charsWritten)
    {
        destination[charsWritten++] = character;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryAppend(Span<char> destination, char character, ref int charsWritten, int buffer = 0)
    {
        if ((1 + charsWritten + buffer) > destination.Length)
        {
            return false;
        }

        Append(destination, character, ref charsWritten);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryAppendJoin(char deliminator, Span<char> destination, ReadOnlySpan<char> source, ref int charsWritten, params ReadOnlySpan<Range> ranges)
    {
        int count = 0;
        int length = ranges.Length - 1;

        while (count < length)
        {
            if (TryAppend(destination, source[ranges[count]], ref charsWritten, 1))
            {
                destination[charsWritten++] = deliminator;
                count++;
                continue;
            }

            return false;
        }

        return TryAppend(destination, source[ranges[count]], ref charsWritten);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int StripWhitespace(ReadOnlySpan<char> input, Span<char> output)
    {
        int charsWritten = 0;

        for (int i = 0; i < input.Length && charsWritten < output.Length; i++)
        {
            ref readonly char c = ref input[i];
            if (c != Character.Whitespace)
            {
                output[charsWritten++] = c;
            }
        }

        return charsWritten;
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
    public static unsafe T ConcatenateBytes<T>(byte* ptr, int length) where T : unmanaged, IUnsignedNumber<T>, IBitwiseOperators<T, T, T>
    {
        T result = default;
        for (int i = 0; i < length; i++)
        {
            result |= T.CreateTruncating((ptr[i]) << (BitsPerByte * i));
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void SpreadBytes<T>(T number, byte* ptr, int length) where T : unmanaged, IUnsignedNumber<T>, IShiftOperators<T, int, T>
    {
        for (int i = 0; i < length; i++)
        {
            ptr[i] = byte.CreateTruncating(number >> BitsPerByte * i);
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
