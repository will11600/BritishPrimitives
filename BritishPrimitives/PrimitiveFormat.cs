using System.Buffers;
using System.Runtime.CompilerServices;

namespace BritishPrimitives;

internal static class PrimitiveFormat
{
    public const char General = 'G';
    public const char Spaced = 'S';

    private static readonly SearchValues<char> _specifiers = SearchValues.Create([General, Spaced]);

    public const int MaxSpecifierCount = 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> format, out char specifier)
    {
        if (format.IsEmpty)
        {
            specifier = General;
            return true;
        }

        specifier = char.ToUpperInvariant(format[0]);
        return format.Length <= MaxSpecifierCount && _specifiers.Contains(specifier);
    }
}
