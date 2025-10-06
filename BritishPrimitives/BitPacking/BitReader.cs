using System.Runtime.CompilerServices;

namespace BritishPrimitives.BitPacking;

internal unsafe readonly ref struct BitReader
{
    public required byte* Value { get; init; }

    public required int ByteLength { get; init; }

    public required int BitLength { get; init; }

    public BitReader(byte* ptr, int length)
    {
        Value = ptr;

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length, nameof(length));
        ByteLength = length;

        BitLength = length * Helpers.BitsPerByte;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadByte(int position, int bitLength)
    {
        int byteIndex = position / Helpers.BitsPerByte;
        int bitOffset = position % Helpers.BitsPerByte;

        ushort bits = Value[byteIndex];
        if (bitOffset + bitLength > Helpers.BitsPerByte)
        {
            bits |= (ushort)(Value[byteIndex + 1] << Helpers.BitsPerByte);
        }
        bits >>= bitOffset;
        int mask = (1 << bitLength) - 1;
        return (byte)(bits & mask);
    }
}
