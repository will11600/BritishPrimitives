using System.Numerics;
using System.Runtime.CompilerServices;

namespace BritishPrimitives.BitPacking;

internal static class AlphanumericBitPacker
{
    private const string AlphanumericChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const uint Base36 = 36;
    private const uint AlphabetOffset = 10;
    private const int MaxLength = 12;

    public static int UnpackLettersAndNumbers(this ref readonly BitReader reader, ref int position, Span<char> chars)
    {
        int count = 0;

        while (count < chars.Length)
        {
            int length = Math.Min(MaxLength, chars.Length - count);
            if (!reader.TryUnpackUInt64(ref position, out ulong value, Helpers.Max((uint)length, Base36)))
            {
                break;
            }

            for (var (q, r) = (value, 0UL); q > 0UL; (q, r) = Math.DivRem(q, Base36))
            {
                chars[^(count++)] = AlphanumericChars[(int)r];
            }
        }

        return count;
    }

    public static int PackLettersAndNumbers(this ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> chars)
    {
        int count = 0;

        while (count < chars.Length)
        {
            ulong encoded = default;

            int length = Math.Min(MaxLength, chars.Length - count);
            for (int i = count; i < length; i++)
            {
                uint normalizedChar;
                switch (chars[i])
                {
                    case char c when Character.IsLowercaseLetter(c):
                        normalizedChar = AlphabetOffset + (uint)Character.Normalize(c, Character.LowercaseA);
                        break;
                    case char c when Character.IsUppercaseLetter(c):
                        normalizedChar = AlphabetOffset + (uint)Character.Normalize(c, Character.UppercaseA);
                        break;
                    case char c when Character.IsDigit(c):
                        normalizedChar = (uint)Character.Normalize(c, Character.Zero);
                        break;
                    default:
                        return count;
                }

                encoded = encoded * Base36 + normalizedChar;
                count++;
            }

            if (!writer.TryPackUInt64(ref position, encoded, Helpers.Max((uint)length, Base36)))
            {
                break;
            }
        }   

        return count;
    }
}