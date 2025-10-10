using System.Runtime.CompilerServices;

namespace BritishPrimitives.BitPacking;

internal static class AlphanumericBitPacker
{
    private delegate bool Normalizer(char value, out int result);

    public const int SizeInBits = 6;

    public const string Characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const int AlphabetOffset = 10;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryPackLetter(this ref readonly BitWriter writer, ref int position, char value)
    {
        return writer.TryPack(ref position, value, TryNormalizeLetter);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryPackDigit(this ref readonly BitWriter writer, ref int position, char value)
    {
        return writer.TryPack(ref position, value, TryNormalizeDigit);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryPackAlphanumeric(this ref readonly BitWriter writer, ref int position, char value)
    {
        return writer.TryPack(ref position, value, TryNormalizeAlphanumeric);
    }

    public static int UnpackAlphanumeric(this ref readonly BitReader reader, ref int position, Span<char> chars)
    {
        int count = 0;

        int length = Helpers.ClampAvailable(in reader, position, SizeInBits, chars.Length);
        for (; count < length && TryUnpackCharacter(in reader, ref position, out char result); count++)
        {
            chars[count] = result;
            position += SizeInBits;
        }

        return count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PackAlphanumeric(this ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> chars)
    {
        return PackAlphanumeric(in writer, ref position, chars, TryNormalizeAlphanumeric);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PackLetters(this ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> chars)
    {
        return PackAlphanumeric(in writer, ref position, chars, TryNormalizeLetter);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PackDigits(this ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> chars)
    {
        return PackAlphanumeric(in writer, ref position, chars, TryNormalizeDigit);
    }

    private static int PackAlphanumeric(this ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> chars, Normalizer normalizer)
    {
        int count = 0;

        int length = Helpers.ClampAvailable(in writer, position, SizeInBits, chars.Length);
        for (; count < length && normalizer(chars[count], out int normalizedChar); count++)
        {
            writer.WriteByte(position, (byte)normalizedChar, SizeInBits);
            position += SizeInBits;
        }

        return count;
    }

    private static bool TryUnpackCharacter(this ref readonly BitReader reader, ref int position, out char result)
    {
        int index = reader.ReadByte(position, SizeInBits);

        if (index < Characters.Length)
        {
            result = Characters[index];
            return true;
        }

        return Helpers.FalseOutDefault(out result);
    }

    private static bool TryPack(this ref readonly BitWriter writer, ref int position, char value, Normalizer normalizer)
    {
        if (writer.CanWrite(position, SizeInBits) && normalizer(value, out int normalizedChar))
        {
            writer.WriteByte(position, (byte)normalizedChar, SizeInBits);
            position += SizeInBits;
            return true;
        }

        return false;
    }

    private static bool TryNormalizeDigit(char value, out int result)
    {
        if (value >= Character.Zero && value <= Character.Nine)
        {
            result = value - Character.Zero;
            return true;
        }

        return Helpers.FalseOutDefault(out result);
    }

    private static bool TryNormalizeLetter(char value, out int result) => value switch
    {
        >= Character.LowercaseA and <= Character.LowercaseZ => Normalize(value, Character.LowercaseA, AlphabetOffset, out result),
        >= Character.UppercaseA and <= Character.UppercaseZ => Normalize(value, Character.UppercaseA, AlphabetOffset, out result),
        _ => Helpers.FalseOutDefault(out result),
    };

    private static bool TryNormalizeAlphanumeric(char value, out int result) => value switch
    {
        >= Character.LowercaseA and <= Character.LowercaseZ => Normalize(value, Character.LowercaseA, AlphabetOffset, out result),
        >= Character.UppercaseA and <= Character.UppercaseZ => Normalize(value, Character.UppercaseA, AlphabetOffset, out result),
        >= Character.Zero and <= Character.Nine => Normalize(value, Character.Zero, out result),
        _ => Helpers.FalseOutDefault(out result)
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Normalize(char value, char start, int offset, out int result)
    {
        result = offset + (value - start);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Normalize(char value, char start, out int result)
    {
        result = value - start;
        return true;
    }
}