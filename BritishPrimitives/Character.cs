using System.Runtime.CompilerServices;

namespace BritishPrimitives;

internal static class Character
{
    public const char UppercaseA = 'A';
    public const char UppercaseZ = 'Z';
    public const char LowercaseA = 'a';
    public const char LowercaseZ = 'z';
    public const char Zero = '0';
    public const char Nine = '9';
    public const char Whitespace = ' ';

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLetter(char c)
    {
        return IsUppercaseLetter(c) || IsLowercaseLetter(c);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsUppercaseLetter(char c)
    {
        return c is >= UppercaseA and <= UppercaseZ;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLowercaseLetter(char c)
    {
        return c is >= LowercaseA and <= LowercaseZ;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDigit(char c)
    {
        return c is >= Zero and <= Nine;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char Offset(int value, char offset)
    {
        return (char)(offset + value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Normalize(char value, char offset)
    {
        return value - offset;
    }

    public static bool ContiguousSequenceWithoutWhitespace(ReadOnlySpan<char> chars, Func<char, bool> predicate, int minLength, int maxLength, out Range range)
    {
        if (maxLength > chars.Length || minLength < 1 || maxLength < minLength)
        {
            return Helpers.FalseOutDefault(out range);
        }

        int startIndex = chars.Length;
        int endIndex = 0;
        int count;

        while ((count = endIndex - startIndex) < maxLength && endIndex < chars.Length)
        {
            ref readonly char c = ref chars[endIndex++];

            if (predicate(c))
            {
                startIndex = Math.Min(startIndex, endIndex);
                continue;
            }

            if (c == Whitespace && count < 1)
            {
                startIndex = endIndex + 1;
                continue;
            }

            return Helpers.FalseOutDefault(out range);
        }

        range = startIndex..endIndex;
        return count == maxLength;
    }

    public static int SequenceWithoutWhitespace(ReadOnlySpan<char> input, Span<char> output, Func<char, bool> predicate)
    {
        int charsWritten = 0;

        for (int i = 0; i < input.Length && i < output.Length; i++)
        {
            ref readonly char c = ref input[i];

            if (predicate(c))
            {
                output[charsWritten++] = c;
                continue;
            }
            
            if (c != Whitespace)
            {
                break;
            }
        }

        return charsWritten;
    }
}