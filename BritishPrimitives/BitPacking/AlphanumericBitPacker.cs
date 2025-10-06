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
}