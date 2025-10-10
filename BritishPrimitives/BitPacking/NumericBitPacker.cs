using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace BritishPrimitives.BitPacking;

internal static class NumericBitPacker
{
    public const int SizeInBits = sizeof(ulong) * Helpers.BitsPerByte;
    private const int Log10MaxInt32PlusOne = 9;

    public static bool TryUnpackInteger(this ref readonly BitReader reader, ref int position, out ulong value, ulong max = ulong.MaxValue)
    {
        value = default;

        int remainingBits = BitOperations.Log2(max);

        if (!reader.CanRead(position, remainingBits))
        {
            return false;
        }

        while (remainingBits > 0)
        {
            int chunkLength = Math.Min(remainingBits, Helpers.BitsPerByte);
            byte chunk = reader.ReadByte(position, chunkLength);

            value <<= chunkLength;
            value |= chunk;

            position += chunkLength;
            remainingBits -= chunkLength;
        }

        return true;
    }

    public static bool TryPackInteger(this ref readonly BitWriter writer, ref int position, ulong value, ulong max = ulong.MaxValue)
    {
        int maxBits = BitOperations.Log2(max);
        int bitsToWrite = Math.Min(maxBits, SizeInBits - BitOperations.LeadingZeroCount(value));

        if (!writer.CanWrite(position, bitsToWrite))
        {
            return false;
        }

        int remainingBits = bitsToWrite;

        while (remainingBits > 0)
        {
            int chunkLength = Math.Min(remainingBits, Helpers.BitsPerByte);

            int shiftAmount = remainingBits - chunkLength;
            byte chunkValue = (byte)(value >> shiftAmount);

            writer.WriteByte(position, chunkValue, chunkLength);

            position += chunkLength;
            remainingBits -= chunkLength;
        }

        position += maxBits - bitsToWrite;

        return true;
    }

    public static bool TryPackInteger(this ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> chars)
    {
        return TryPackInteger(in writer, ref position, chars, NumberStyles.None, CultureInfo.InvariantCulture);
    }

    public static bool TryPackInteger(this ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> chars, IFormatProvider? formatProvider)
    {
        return TryPackInteger(in writer, ref position, chars, NumberStyles.None, formatProvider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryPackInteger(this ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> chars, NumberStyles styles, IFormatProvider? formatProvider = null)
    {
        ulong max = MaxValueForDigitCount(chars.Length);
        return uint.TryParse(chars, styles, formatProvider, out uint result) && writer.TryPackInteger(ref position, result, max);
    }

    public static int UnpackInteger(this ref readonly BitReader reader, ref int position, Span<char> chars)
    {
        return UnpackInteger(in reader, ref position, chars, CultureInfo.InvariantCulture);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int UnpackInteger(this ref readonly BitReader reader, ref int position, Span<char> chars, IFormatProvider? formatProvider)
    {
        ulong max = MaxValueForDigitCount(chars.Length);
        if (TryUnpackInteger(in reader, ref position, out ulong result, max) && TryFormatDigits(result, chars, formatProvider, out int charsWritten))
        {
            return charsWritten;
        }

        return default;
    }

    private static bool TryFormatDigits(ulong value, Span<char> chars, IFormatProvider? formatProvider, out int charsWritten)
    {
        charsWritten = 0;
        int offset = 0;
        Span<char> format = stackalloc char[Log10MaxInt32PlusOne];
        format[offset++] = 'D';
        if (chars.Length.TryFormat(format[offset..], out int formatCharsWritten))
        {
            offset += formatCharsWritten;
            return value.TryFormat(chars, out charsWritten, format[..offset], formatProvider);
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong MaxValueForDigitCount(int length)
    {
        return Helpers.Pow((uint)length, 10U) - 1U;
    }
}
