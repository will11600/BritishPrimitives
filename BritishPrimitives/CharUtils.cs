using System.Runtime.CompilerServices;

namespace BritishPrimitives;

internal static class CharUtils
{
    public const string AlphanumericChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static ReadOnlySpan<char> AlphabeticalChars => AlphanumericChars.AsSpan(10);

    public static ReadOnlySpan<char> NumericalChars => AlphanumericChars.AsSpan(0, 10);

    public static bool TryEncodeAlphanumeric(ref readonly char c, out int result)
    {
        switch (c)
        {
            case >= '0' and <= '9':
                result = c - '0';
                return true;
            case >= 'A' and <= 'Z':
                result = c - 'A' + 10;
                return true;
            case >= 'a' and <= 'z':
                result = c - 'a' + 10;
                return true;
            default:
                result = default;
                return false;
        }
    }

    public static bool TryEncodeAlphabetical(ref readonly char c, out int result)
    {
        switch (c)
        {
            case >= 'A' and <= 'Z':
                result = c - 'A' + 10;
                return true;
            case >= 'a' and <= 'z':
                result = c - 'a' + 10;
                return true;
            default:
                result = default;
                return false;
        }
    }

    public static bool TryEncodeNumeric(ref readonly char c, out int result)
    {
        if (c is >= '0' and <= '9')
        {
            result = c - '0';
            return true;
        }

        result = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int TrimAndMakeUpperInvariant(ReadOnlySpan<char> input, Span<char> output)
    {
        int length = 0;
        for (int i = 0; i < input.Length; i++)
        {
            ref readonly char c = ref input[i];
            if (!char.IsWhiteSpace(c))
            {
                output[length++] = char.ToUpperInvariant(c);
            }
        }
        return length;
    }
}
