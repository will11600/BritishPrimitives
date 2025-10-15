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

        if (!Helpers.HasAvailableBits(in reader, position, remainingBits))
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

        if (!Helpers.HasAvailableBits(in writer, position, maxBits))
        {
            return false;
        }

        int remainingBits = maxBits;

        while (remainingBits > 0)
        {
            int chunkLength = Math.Min(remainingBits, Helpers.BitsPerByte);

            int shiftAmount = remainingBits - chunkLength;
            byte chunkValue = (byte)(value >> shiftAmount);

            writer.WriteByte(position, chunkValue, chunkLength);

            position += chunkLength;
            remainingBits -= chunkLength;
        }

        return true;
    }

    public static bool TryPackInteger(this ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> chars)
    {
        return TryPackInteger(in writer, ref position, chars, out _, NumberStyles.None, CultureInfo.InvariantCulture);
    }

    public static bool TryPackInteger(this ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> chars, out ulong result)
    {
        return TryPackInteger(in writer, ref position, chars, out result, NumberStyles.None, CultureInfo.InvariantCulture);
    }

    public static bool TryPackInteger(this ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> chars, IFormatProvider? formatProvider)
    {
        return TryPackInteger(in writer, ref position, chars, out _, NumberStyles.None, formatProvider);
    }

    public static bool TryPackInteger(this ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> chars, out ulong result, IFormatProvider? formatProvider)
    {
        return TryPackInteger(in writer, ref position, chars, out result, NumberStyles.None, formatProvider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryPackInteger(this ref readonly BitWriter writer,
                                      ref int position,
                                      ReadOnlySpan<char> chars,
                                      out ulong result,
                                      NumberStyles styles,
                                      IFormatProvider? formatProvider = null)
    {
        ulong max = MaxValueForDigitCount(chars.Length);
        return ulong.TryParse(chars, styles, formatProvider, out result) && writer.TryPackInteger(ref position, result, max);
    }

    public static int UnpackInteger(this ref readonly BitReader reader, ref int position, Span<char> chars)
    {
        return UnpackInteger(in reader, ref position, chars, CultureInfo.InvariantCulture);
    }

    public static int UnpackInteger(this ref readonly BitReader reader, ref int position, Span<char> chars, ReadOnlySpan<char> format)
    {
        return UnpackInteger(in reader, ref position, chars, format, CultureInfo.InvariantCulture);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int UnpackInteger(this ref readonly BitReader reader, ref int position, Span<char> chars, IFormatProvider? formatProvider)
    {
        Span<char> format = stackalloc char[Log10MaxInt32PlusOne];
        if (TryCreateDefaultFormat(chars.Length, format, out int formatLength))
        {
            return UnpackInteger(in reader, ref position, chars, format[..formatLength], formatProvider);
        }

        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int UnpackInteger(this ref readonly BitReader reader, ref int position, Span<char> chars, ReadOnlySpan<char> format, IFormatProvider? formatProvider)
    {
        ulong max = MaxValueForDigitCount(chars.Length);
        if (TryUnpackInteger(in reader, ref position, out ulong result, max) && result.TryFormat(chars, out int charsWritten, format, formatProvider))
        {
            return charsWritten;
        }

        return default;
    }

    private static bool TryCreateDefaultFormat(int length, Span<char> destination, out int charsWritten)
    {
        charsWritten = 0;
        destination[charsWritten++] = 'D';
        if (length.TryFormat(destination[charsWritten..], out int formatCharsWritten))
        {
            charsWritten += formatCharsWritten;
            return true;
        }
        return false;
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
        return Helpers.Pow(10U, (uint)length) - 1U;
    }
}
