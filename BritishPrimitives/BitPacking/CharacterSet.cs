namespace BritishPrimitives.BitPacking;

internal static class CharacterSet
{
    public static AlphabeticalCharacterSet Alphabetical { get; } = new AlphabeticalCharacterSet();

    public static AlphanumericCharacterSet Alphanumeric { get; } = new AlphanumericCharacterSet();
}
