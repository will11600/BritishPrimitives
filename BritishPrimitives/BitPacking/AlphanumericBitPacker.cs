namespace BritishPrimitives.BitPacking;

internal static class AlphanumericBitPacker
{
    public const int SizeInBits = 6;

    private const string AlphanumericChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int AlphabetOffset = 10;

    public static int UnpackLettersAndNumbers(this ref readonly BitReader reader, ref int position, Span<char> chars)
    {
        int count = 0;

        if (!reader.CanRead(position, chars.Length * SizeInBits))
        {
            return count;
        }

        while (count < chars.Length)
        {
            chars[count++] = AlphanumericChars[reader.ReadByte(position, SizeInBits)];
            position += SizeInBits;
        }

        return count;
    }

    public static int PackLettersAndNumbers(this ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> chars)
    {
        int count = 0;

        if (!writer.CanWrite(position, chars.Length * SizeInBits))
        {
            return count;
        }

        for (int i = count; i < chars.Length; i++)
        {
            int normalizedChar;
            switch (chars[i])
            {
                case char c when Character.IsLowercaseLetter(c):
                    normalizedChar = AlphabetOffset + Character.Normalize(c, Character.LowercaseA);
                    break;
                case char c when Character.IsUppercaseLetter(c):
                    normalizedChar = AlphabetOffset + Character.Normalize(c, Character.UppercaseA);
                    break;
                case char c when Character.IsDigit(c):
                    normalizedChar = Character.Normalize(c, Character.Zero);
                    break;
                default:
                    return count;
            }

            writer.WriteByte(position, (byte)normalizedChar, SizeInBits);

            position += SizeInBits;
            count++;
        }

        return count;
    }

    public static bool TryPackAlphanumericLetter(this ref readonly BitWriter writer, ref int position, char letter)
    {
        if (writer.CanWrite(position, SizeInBits) && Character.TryNormalizeLetter(letter, out int normalizedChar))
        {
            writer.WriteByte(position, (byte)(normalizedChar + AlphabetOffset), SizeInBits);
            position += SizeInBits;
            return true;
        }
        
        return false;
    }

    public static int PackAlphanumericLetters(this ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> chars)
    {
        int count = 0;

        if (!writer.CanWrite(position, chars.Length * SizeInBits))
        {
            return count;
        }

        for (; count < chars.Length; count++)
        {
            if (Character.TryNormalizeDigit(chars[count], out int normalizedChar))
            {
                writer.WriteByte(position, (byte)normalizedChar, SizeInBits);
                position += SizeInBits;
                continue;
            }

            return count;
        }

        return count;
    }

    public static bool TryPackAlphanumericDigit(this ref readonly BitWriter writer, ref int position, char letter)
    {
        if (writer.CanWrite(position, SizeInBits) && Character.TryNormalizeDigit(letter, out int normalizedChar))
        {
            writer.WriteByte(position, (byte)normalizedChar, SizeInBits);
            position += SizeInBits;
            return true;
        }
        
        return false;
    }

    public static bool TryPackAlphanumeric(this ref readonly BitWriter writer, ref int position, char letter)
    {
        if (!writer.CanWrite(position, SizeInBits))
        {
            return false;
        }

        int normalizedChar;
        switch (letter)
        {
            case char c when Character.IsUppercaseLetter(c):
                normalizedChar = Character.Normalize(letter, Character.UppercaseA) + AlphabetOffset;
                break;
            case char c when Character.IsLowercaseLetter(c):
                normalizedChar = Character.Normalize(letter, Character.LowercaseA) + AlphabetOffset;
                break;
            case char c when Character.IsDigit(c):
                normalizedChar = Character.Normalize(letter, Character.Zero);
                break;
            default:
                return false;
        }

        writer.WriteByte(position, (byte)normalizedChar, SizeInBits);
        position += SizeInBits;
        return true;
    }
}