namespace BritishPrimitives.BitPacking;

internal static class AlphabeticBitPacker
{
    public const int SizeInBits = 5;

    public static int UnpackLetters(this ref readonly BitReader reader, ref int position, Span<char> chars)
    {
        int count = 0;

        if (!reader.CanRead(position, chars.Length * SizeInBits))
        {
            return count;
        }

        while (count < chars.Length)
        {
            chars[count++] = Character.Offset(reader.ReadByte(position, SizeInBits), Character.UppercaseA);
            position += SizeInBits;
        }

        return count;
    }

    public static int PackLetters(this ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> chars)
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
                    normalizedChar = Character.Normalize(c, Character.LowercaseA);
                    break;
                case char c when Character.IsUppercaseLetter(c):
                    normalizedChar = Character.Normalize(c, Character.UppercaseA);
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
