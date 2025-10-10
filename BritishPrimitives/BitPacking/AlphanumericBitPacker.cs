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

        if (!reader.CanRead(position, chars.Length * SizeInBits))
        {
            return count;
        }

        while (count < chars.Length)
        {
            chars[count++] = Characters[reader.ReadByte(position, SizeInBits)];
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

        if (!writer.CanWrite(position, chars.Length * SizeInBits))
        {
            return count;
        }

        for (int i = count; i < chars.Length && normalizer(chars[i], out int normalizedChar); i++)
        {
            writer.WriteByte(position, (byte)normalizedChar, SizeInBits);
            position += SizeInBits;
            count++;
        }

        return count;
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

    private static bool TryNormalizeLetter(char value, out int result)
    {
        switch (value)
        {
            case >= Character.LowercaseA and <= Character.LowercaseZ:
                result = AlphabetOffset + (value - Character.LowercaseA);
                return true;
            case >= Character.UppercaseA and <= Character.UppercaseZ:
                result = AlphabetOffset + (value - Character.UppercaseA);
                return true;
            default:
                return Helpers.FalseOutDefault(out result);
        }
    }

    private static bool TryNormalizeAlphanumeric(char value, out int result)
    {
        switch (value)
        {
            case >= Character.LowercaseA and <= Character.LowercaseZ:
                result = AlphabetOffset + (value - Character.LowercaseA);
                return true;
            case >= Character.UppercaseA and <= Character.UppercaseZ:
                result = AlphabetOffset + (value - Character.UppercaseA);
                return true;
            case >= Character.Zero and <= Character.Nine:
                result = value - Character.Zero;
                return true;
            default:
                return Helpers.FalseOutDefault(out result);
        }
    }
}