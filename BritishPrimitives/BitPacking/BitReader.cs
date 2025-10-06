using System.Runtime.CompilerServices;

namespace BritishPrimitives.BitPacking;

internal unsafe readonly ref struct BitReader
{
    public required byte* Value { get; init; }

    public required int ByteLength { get; init; }

    public required int BitLength { get; init; }

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitReader Create(byte* ptr, int byteLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(byteLength, nameof(byteLength));
        return new BitReader()
        {
            Value = ptr,
            ByteLength = byteLength,
            BitLength = byteLength * Helpers.BitsPerByte
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitReader Create<T>(ref T value) where T : unmanaged
    {
        int byteLength = sizeof(T);
        return new BitReader()
        {
            Value = (byte*)Unsafe.AsPointer(ref value),
            ByteLength = byteLength,
            BitLength = byteLength * Helpers.BitsPerByte
        };
    }
}
