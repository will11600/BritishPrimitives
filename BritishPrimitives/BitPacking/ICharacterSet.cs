namespace BritishPrimitives.BitPacking;

internal interface ICharacterSet
{
    public int SizeInBits { get; }

    bool TryNormalize(char value, out int result);

    bool TryOffset(int value, out char result);
}
