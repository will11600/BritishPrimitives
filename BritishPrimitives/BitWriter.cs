using System.Runtime.CompilerServices;
using static BritishPrimitives.CharUtils;

namespace BritishPrimitives;

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
        if (position < 0 || length < 1 || position > _bitLength)
        {
            return false;
        }

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

        position += length;

        return true;
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
}
