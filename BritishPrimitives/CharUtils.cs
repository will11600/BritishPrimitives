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
}
