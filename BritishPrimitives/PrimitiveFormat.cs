using System.Runtime.CompilerServices;

namespace BritishPrimitives;

internal static class PrimitiveFormat
{
    public const string Specifiers = "GS";

    public static char General => Specifiers[0];

    public static char Spaced => Specifiers[1];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse(ReadOnlySpan<char> format, out char specifier)
    {
        specifier = format.IsEmpty ? General : char.ToUpperInvariant(format[0]);
        return format.Length <= 1 && Specifiers.Contains(specifier);
    }
}
