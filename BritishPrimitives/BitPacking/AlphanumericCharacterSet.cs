namespace BritishPrimitives.BitPacking;

internal sealed class AlphanumericCharacterSet : ICharacterSet
{
    private const string Characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int AlphabetOffset = 10;

    public int SizeInBits => 6;

    public bool TryNormalize(char value, out int result) => value switch
    {
        >= Character.Zero and <= Character.Nine => Normalize(value, Character.Zero, out result),
        >= Character.UppercaseA and <= Character.UppercaseZ => Normalize(value, Character.UppercaseA, out result, AlphabetOffset),
        >= Character.LowercaseA and <= Character.LowercaseZ => Normalize(value, Character.LowercaseA, out result, AlphabetOffset),
        _ => Helpers.FalseOutDefault(out result),
    };

    public bool TryOffset(int value, out char result)
    {
        if (value >= 0 && value < Characters.Length)
        {
            result = Characters[value];
            return true;
        }

        return Helpers.FalseOutDefault(out result);
    }

    private static bool Normalize(char value, char start, out int result, int offset = 0)
    {
        result = value - start + offset;
        return true;
    }
}
