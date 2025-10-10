namespace BritishPrimitives.BitPacking;

internal unsafe interface IBitPacker
{
    public byte* Value { get; }

    public int ByteLength { get; }

    public int BitLength { get; }
}
