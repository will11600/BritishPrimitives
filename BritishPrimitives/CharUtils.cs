using System.Runtime.CompilerServices;

namespace BritishPrimitives;

internal static class CharUtils
{
    public const int BitsPerByte = 8;
    public const int LetterBits = 5;
    public const int DigitBits = 4;
    public const int AlphanumericBits = 6;

    public const string AlphanumericChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const int AlphabetOffset = 10;

    public const char UppercaseA = 'A';
    public const char UppercaseZ = 'Z';
    public const char LowercaseA = 'a';
    public const char LowercaseZ = 'z';
    public const char Zero = '0';
    public const char Nine = '9';
    public const char Whitespace = ' ';

    public static ReadOnlySpan<char> AlphabeticalChars => AlphanumericChars.AsSpan(AlphabetOffset);

    public static ReadOnlySpan<char> NumericalChars => AlphanumericChars.AsSpan(0, AlphabetOffset);

    public static bool TryEncodeAlphanumeric(ref readonly char c, out int result)
    {
        switch (c)
        {
            case >= Zero and <= Nine:
                result = c - Zero;
                return true;
            case >= UppercaseA and <= UppercaseZ:
                result = c - UppercaseA + AlphabetOffset;
                return true;
            case >= LowercaseA and <= LowercaseZ:
                result = c - LowercaseA + AlphabetOffset;
                return true;
            default:
                result = default;
                return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLetter(char c) => c switch
    {
        >= UppercaseA and <= UppercaseZ => true,
        >= LowercaseA and <= LowercaseZ => true,
        _ => false
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDigit(char c) => c is >= Zero and <= Nine;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool FalseOutDefault<T>(out T value) where T : struct
    {
        value = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte UppercaseEncode(char c)
    {
        return (byte)(c - UppercaseA);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte NumeralEncode(char c)
    {
        return (byte)(c - Zero);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseAlphanumericUpperInvariant(ReadOnlySpan<char> input, Span<char> output, int minLength, int maxLength, out int charsWritten)
    {
        charsWritten = AlphanumericUpperInvariant(input, output);
        return charsWritten > minLength && charsWritten < maxLength;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParseAlphanumericUpperInvariant(ReadOnlySpan<char> input, Span<char> output, int length, out int charsWritten)
    {
        charsWritten = AlphanumericUpperInvariant(input, output);
        return charsWritten == length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int AlphanumericUpperInvariant(ReadOnlySpan<char> input, Span<char> output)
    {
        int charsWritten = default;

        for (int i = 0; i < input.Length && i < output.Length; i++)
        {
            ref readonly char c = ref input[i];

            switch (c)
            {
                case >= UppercaseA and <= UppercaseZ:
                case >= Zero and <= Nine:
                    output[charsWritten++] = c;
                    break;
                case >= LowercaseA and <= LowercaseZ:
                    output[charsWritten++] = char.ToUpperInvariant(c);
                    break;
                case Whitespace:
                    break;
                default:
                    return charsWritten;
            }
        }

        return charsWritten;
    }
}