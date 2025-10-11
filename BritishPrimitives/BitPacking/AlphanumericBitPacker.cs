using System.Runtime.CompilerServices;

namespace BritishPrimitives.BitPacking;

internal static class AlphanumericBitPacker
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryPackLetter(this ref readonly BitWriter writer, ref int position, char value)
    {
        return writer.TryPack(ref position, value, CharacterSet.Alphanumeric);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryPackLetter(this ref readonly BitWriter writer, ref int position, char value, ICharacterSet characterSet)
    {
        return writer.TryPack(ref position, value, characterSet);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryPackDigit(this ref readonly BitWriter writer, ref int position, char value)
    {
        return writer.TryPack(ref position, value, CharacterSet.Alphanumeric);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryPackAlphanumeric(this ref readonly BitWriter writer, ref int position, char value)
    {
        return writer.TryPack(ref position, value, CharacterSet.Alphanumeric);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int UnpackLetters(this ref readonly BitReader reader, ref int position, Span<char> chars)
    {
        return UnpackCharacters(in reader, ref position, chars, CharacterSet.Alphabetical);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int UnpackAlphanumeric(this ref readonly BitReader reader, ref int position, Span<char> chars)
    {
        return UnpackCharacters(in reader, ref position, chars, CharacterSet.Alphanumeric);
    }

    public static int UnpackCharacters(ref readonly BitReader reader, ref int position, Span<char> chars, ICharacterSet characterSet)
    {
        int count = 0;

        int length = Helpers.ClampAvailableBits(in reader, position, characterSet.SizeInBits, chars.Length);
        for (; count < length && TryUnpackCharacter(in reader, ref position, characterSet, out char result); count++)
        {
            chars[count] = result;
            position += characterSet.SizeInBits;
        }

        return count;
    }

    private static bool TryUnpackCharacter(in BitReader reader, ref int position, ICharacterSet characterSet, out char result)
    {
        int charByte = reader.ReadByte(position, characterSet.SizeInBits);
        return characterSet.TryOffset(charByte - 1, out result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PackAlphanumeric(this ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> chars)
    {
        return Pack(in writer, ref position, chars, CharacterSet.Alphanumeric);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PackLetters(this ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> chars, ICharacterSet characterSet)
    {
        return Pack(in writer, ref position, chars, characterSet);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PackLetters(this ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> chars)
    {
        return Pack(in writer, ref position, chars, CharacterSet.Alphanumeric);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PackDigits(this ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> chars)
    {
        return Pack(in writer, ref position, chars, CharacterSet.Alphanumeric);
    }

    public static int Pack(ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> chars, ICharacterSet characterSet)
    {
        int count = 0;

        int length = Helpers.ClampAvailableBits(in writer, position, characterSet.SizeInBits, chars.Length);
        for (; count < length && characterSet.TryNormalize(chars[count], out int normalizedChar); count++)
        {
            writer.WriteByte(position, (byte)(normalizedChar + 1), characterSet.SizeInBits);
            position += characterSet.SizeInBits;
        }

        return count;
    }

    public static bool TryPack(this ref readonly BitWriter writer, ref int position, char value, ICharacterSet characterSet)
    {
        if (Helpers.HasAvailableBits(in writer, position, characterSet.SizeInBits) && characterSet.TryNormalize(value, out int normalizedChar))
        {
            writer.WriteByte(position, (byte)(normalizedChar + 1), characterSet.SizeInBits);
            position += characterSet.SizeInBits;
            return true;
        }

        return false;
    }
}