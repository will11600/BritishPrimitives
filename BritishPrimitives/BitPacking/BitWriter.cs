using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using static BritishPrimitives.CharUtils;

namespace BritishPrimitives.BitPacking;

internal unsafe readonly ref struct BitWriter
{
    private readonly byte* _ptr;
    private readonly int _byteLength;
    private readonly int _bitLength;

    public BitWriter(byte* ptr, int length)
    {
        _ptr = ptr;

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length, nameof(length));
        _byteLength = length;

        _bitLength = length * BitsPerByte;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryWriteBits(ref int position, byte value, int length)
    {
        if (CanWrite(position, length))
        {
            WriteBits(position, value, length);
            position += length;
            return true;
        }

        return length == 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteBits(int position, byte value, int length)
    {
        int byteIndex = position / BitsPerByte;
        int bitOffset = position % BitsPerByte;

        ushort valueMask = (ushort)((1 << length) - 1);
        ushort clearMask = (ushort)~(valueMask << bitOffset);

        ushort preparedValue = (ushort)((value & valueMask) << bitOffset);

        for (int i = 0; i < 2 && byteIndex < _byteLength; i++)
        {
            ref byte byteRef = ref _ptr[byteIndex++];
            int shiftAmount = BitsPerByte * i;
            byteRef &= (byte)(clearMask >> shiftAmount);
            byteRef |= (byte)(preparedValue >> shiftAmount);
        }
    }

    public bool TryWriteAlphanumeric(ref int position, char value) => value switch
    {
        >= Zero and <= Nine => TryWriteBits(ref position, EncodeChar(value, Zero), AlphanumericBits),
        >= UppercaseA and <= UppercaseZ => TryWriteBits(ref position, EncodeChar(value, UppercaseA, AlphabetOffset), AlphanumericBits),
        >= LowercaseA and <= LowercaseZ => TryWriteBits(ref position, EncodeChar(value, LowercaseA, AlphabetOffset), AlphanumericBits),
        _ => false
    };

    public bool TryWriteAlphanumeric(ref int position, ReadOnlySpan<char> chars)
    {
        for (int i = 0; i < chars.Length; i++)
        {
            if (TryWriteAlphanumeric(ref position, chars[i]))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    public bool TryWriteDigit(ref int position, char value) => value switch
    {
        >= Zero and <= Nine => TryWriteBits(ref position, EncodeChar(value, Zero), DigitBits),
        _ => false
    };

    public bool TryWriteDigits(ref int position, ReadOnlySpan<char> chars)
    {
        for (int i = 0; i < chars.Length; i++)
        {
            if (TryWriteDigit(ref position, chars[i]))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    public bool TryWriteNumber(ref int position, ReadOnlySpan<char> chars, int length)
    {
        return ulong.TryParse(chars, NumberStyles.None, CultureInfo.InvariantCulture, out ulong result) && TryWriteNumber(ref position, result, length);
    }

    public bool TryWriteNumber(ref int position, ulong number, int length)
    {
        const int UInt64SizeInBits = sizeof(ulong) * BitsPerByte;
        return TryWriteNumber(ref position, shift => (byte)(number >> shift), length, UInt64SizeInBits);
    }

    public bool TryWriteNumber(ref int position, uint number, int length)
    {
        const int UInt32SizeInBits = sizeof(uint) * BitsPerByte;
        return TryWriteNumber(ref position, shift => (byte)(number >> shift), length, UInt32SizeInBits);
    }

    public bool TryWriteNumber(ref int position, ushort number, int length)
    {
        const int UInt16SizeInBits = sizeof(ushort) * BitsPerByte;
        return TryWriteNumber(ref position, shift => (byte)(number >> shift), length, UInt16SizeInBits);
    }

    private bool TryWriteNumber(ref int position, Func<int, byte> rightShift, int length, int maxLength)
    {
        if (length > maxLength || !CanWrite(position, length))
        {
            return false;
        }

        int remainingBits = length;
        while (remainingBits > 0)
        {
            int chunkLength = Math.Min(remainingBits, BitsPerByte);

            int shiftAmount = remainingBits - chunkLength;
            byte chunkValue = rightShift(shiftAmount);

            WriteBits(position, chunkValue, chunkLength);

            position += chunkLength;
            remainingBits -= chunkLength;
        }

        return true;
    }

    public bool TryWriteLetter(ref int position, char value) => value switch
    {
        >= UppercaseA and <= UppercaseZ => TryWriteBits(ref position, EncodeChar(value, UppercaseA), LetterBits),
        >= LowercaseA and <= LowercaseZ => TryWriteBits(ref position, EncodeChar(value, LowercaseA), LetterBits),
        _ => false
    };

    public bool TryWriteLetters(ref int position, ReadOnlySpan<char> chars)
    {
        for (int i = 0; i < chars.Length; i++)
        {
            if (TryWriteLetter(ref position, chars[i]))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    public bool TryWriteBit(ref int position, bool value) => TryWriteBits(ref position, value ? byte.MaxValue : byte.MinValue, 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte EncodeChar(char value, char start, int offset = 0) => (byte)(value - start + offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CanWrite(int position, int length)
    {
        return position >= 0 && length >= 1 && position + length <= _bitLength;
    }
}
