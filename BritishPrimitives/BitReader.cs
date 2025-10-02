using System.Runtime.CompilerServices;
using static BritishPrimitives.CharUtils;

namespace BritishPrimitives;

internal unsafe readonly ref struct BitReader
{
    private readonly byte* _ptr;
    private readonly int _byteLength;
    private readonly int _bitLength;

    public BitReader(byte* ptr, int length)
    {
        _ptr = ptr;

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length, nameof(length));
        _byteLength = length;

        _bitLength = length * BitsPerByte;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadBits(ref int position, int length, out byte result)
    {
        if (position < 0 || length < 1 || length > BitsPerByte || position > _bitLength)
        {
            return FalseOutDefault(out result);
        }

        int byteIndex = position / BitsPerByte;
        int bitOffset = position % BitsPerByte;

        ushort bits = _ptr[byteIndex];
        if (bitOffset + length > BitsPerByte)
        {
            bits |= (ushort)(_ptr[byteIndex + 1] << BitsPerByte);
        }
        bits >>= bitOffset;
        int mask = (1 << length) - 1;
        result = (byte)(bits & mask);

        position += length;

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryReadChar(ref int position, int length, ReadOnlySpan<char> chars, out char result)
    {
        if (TryReadBits(ref position, length, out byte index) && index < chars.Length)
        {
            result = chars[index];
            return true;
        }

        return FalseOutDefault(out result);
    }

    public bool TryReadAlphanumeric(ref int position, out char result) => TryReadChar(ref position, AlphanumericBits, AlphanumericChars, out result);

    public bool TryReadAlphanumeric(ref int position, Span<char> chars, out int charsWritten)
    {
        ReadOnlySpan<char> characters = AlphanumericChars.AsSpan();
        for (charsWritten = 0; charsWritten < chars.Length; charsWritten++)
        {
            if (TryReadChar(ref position, AlphanumericBits, characters, out char result))
            {
                chars[charsWritten] = result;
                continue;
            }

            return false;
        }

        return true;
    }

    public bool TryReadDigit(ref int position, out char result) => TryReadChar(ref position, DigitBits, NumericalChars, out result);

    public bool TryReadDigits(ref int position, Span<char> chars, out int charsWritten)
    {
        ReadOnlySpan<char> characters = NumericalChars;
        for (charsWritten = 0; charsWritten < chars.Length; charsWritten++)
        {
            if (TryReadChar(ref position, DigitBits, characters, out char result))
            {
                chars[charsWritten] = result;
                continue;
            }

            return false;
        }

        return true;
    }

    public bool TryReadLetter(ref int position, out char value) => TryReadChar(ref position, LetterBits, AlphabeticalChars, out value);

    public bool TryReadLetters(ref int position, Span<char> chars, out int charsWritten)
    {
        ReadOnlySpan<char> characters = AlphabeticalChars;
        for (charsWritten = 0; charsWritten < chars.Length; charsWritten++)
        {
            if (TryReadChar(ref position, LetterBits, characters, out char result))
            {
                chars[charsWritten] = result;
                continue;
            }

            return false;
        }

        return true;
    }

    public bool TryReadBit(ref int position, out bool result)
    {
        if (TryReadBits(ref position, 1, out byte bit))
        {
            result = bit > 0;
            return true;
        }

        return FalseOutDefault(out result);
    }
}
