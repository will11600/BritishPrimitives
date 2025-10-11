namespace BritishPrimitives.BitPacking;

internal sealed class AlphabeticalCharacterSet : ICharacterSet
{
    public int SizeInBits => 5;

    public bool TryNormalize(char value, out int result) => value switch
    {
        >= Character.LowercaseA and <= Character.LowercaseZ => Normalize(value, Character.LowercaseA, out result),
        >= Character.UppercaseA and <= Character.UppercaseZ => Normalize(value, Character.UppercaseA, out result),
        _ => Helpers.FalseOutDefault(out result),
    };

    public bool TryOffset(int value, out char result)
    {
        result = (char)(value + Character.UppercaseA);
        return result >= Character.UppercaseA && result <= Character.UppercaseZ;
    }

    private static bool Normalize(char value, char start, out int result)
    {
        result = value - start;
        return true;
    }
}
