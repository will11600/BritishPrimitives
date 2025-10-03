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
    private byte ReadBits(int position, int length)
    {
        int byteIndex = position / BitsPerByte;
        int bitOffset = position % BitsPerByte;

        ushort bits = _ptr[byteIndex];
        if (bitOffset + length > BitsPerByte)
        {
            bits |= (ushort)(_ptr[byteIndex + 1] << BitsPerByte);
        }
        bits >>= bitOffset;
        int mask = (1 << length) - 1;
        return (byte)(bits & mask);
    }

    public bool TryReadBits(ref int position, int length, out byte result)
    {
        if (CanWrite(position, length))
        {
            result = ReadBits(position, length);
            position += length;
            return true;
        }

        return FalseOutDefault(out result);
    }

    public bool TryRead<T>(ref int position, int length, out T result) where T : unmanaged
    {
        result = default;

        if (length > (sizeof(T) * BitsPerByte) || !CanWrite(position, length))
        {
            return false;
        }

        ulong value = 0;
        int bitsRead = 0;

        while (bitsRead < length)
        {
            int chunkLength = Math.Min(length - bitsRead, BitsPerByte);
            byte chunk = ReadBits(position, chunkLength);

            value <<= chunkLength;
            value |= chunk;

            position += chunkLength;
            bitsRead += chunkLength;
        }

        Unsafe.As<T, ulong>(ref result) = value;

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

    public bool TryReadAlphanumeric(ref int position, Span<char> chars, ref int charsWritten)
    {
        ReadOnlySpan<char> characters = AlphanumericChars.AsSpan();
        for (int i = 0; i < chars.Length; i++)
        {
            if (TryReadChar(ref position, AlphanumericBits, characters, out char result))
            {
                chars[i] = result;
                charsWritten++;
                continue;
            }

            return false;
        }

        return true;
    }

    public bool TryReadDigit(ref int position, out char result) => TryReadChar(ref position, DigitBits, NumericalChars, out result);

    public bool TryReadDigits(ref int position, Span<char> chars, ref int charsWritten)
    {
        ReadOnlySpan<char> characters = NumericalChars;
        for (int i = 0; i < chars.Length; i++)
        {
            if (TryReadChar(ref position, DigitBits, characters, out char result))
            {
                chars[i] = result;
                charsWritten++;
                continue;
            }

            return false;
        }

        return true;
    }

    public bool TryReadLetter(ref int position, out char value) => TryReadChar(ref position, LetterBits, AlphabeticalChars, out value);

    public bool TryReadLetters(ref int position, Span<char> chars, ref int charsWritten)
    {
        ReadOnlySpan<char> characters = AlphabeticalChars;
        for (int i = 0; i < chars.Length; i++)
        {
            if (TryReadChar(ref position, LetterBits, characters, out char result))
            {
                chars[i] = result;
                charsWritten++;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bit ReadBit(ref int position)
    {
        return TryReadBits(ref position, 1, out byte bit) ? (Bit)bit : Bit.Error;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CanWrite(int position, int length)
    {
        return position >= 0 && length >= 1 && (position + length) <= _bitLength;
    }
}
