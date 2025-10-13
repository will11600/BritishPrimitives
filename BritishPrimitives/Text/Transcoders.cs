namespace BritishPrimitives.Text;

internal static class Transcoders
{
    public static CharacterTranscoder Alphabetical { get; }

    public static CharacterTranscoder Alphanumeric { get; }

    public static CharacterTranscoder AlphanumericDigits { get; }

    public static CharacterTranscoder AlphanumericLetters { get; }

    static Transcoders()
    {
        Alphabetical = new CharacterTranscoder([Character.UppercaseAToZ], [Character.LowercaseAToZ, Character.UppercaseAToZ]);

        var uppercaseAToZAlphanumeric = Character.UppercaseAToZ with { Offset = Character.ZeroToNine.Count };
        var lowercaseAToZAlphanumeric = Character.LowercaseAToZ with { Offset = Character.ZeroToNine.Count };

        ReadOnlySpan<CharacterRange> alphanumericOutput = [uppercaseAToZAlphanumeric, Character.ZeroToNine];
        Alphanumeric = new CharacterTranscoder(alphanumericOutput, [uppercaseAToZAlphanumeric, lowercaseAToZAlphanumeric, Character.ZeroToNine]);
        AlphanumericDigits = new CharacterTranscoder(alphanumericOutput, [Character.ZeroToNine]);
        AlphanumericLetters = new CharacterTranscoder(alphanumericOutput, [uppercaseAToZAlphanumeric, lowercaseAToZAlphanumeric]);
    }
}
