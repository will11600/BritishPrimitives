using System.Runtime.CompilerServices;

namespace BritishPrimitives.BitPacking;

internal unsafe readonly ref struct BitWriter
{
    public required byte* Value { get; init; }

    public required int ByteLength { get; init; }

    public required long BitLength { get; init; }

    public BitWriter(byte* ptr, int length)
    {
        Value = ptr;

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length, nameof(length));
        ByteLength = length;

        BitLength = length * Helpers.BitsPerByte;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByte(int position, byte value, int bitLength)
    {
        int byteIndex = position / Helpers.BitsPerByte;
        int bitOffset = position % Helpers.BitsPerByte;

        ushort valueMask = (ushort)((1 << bitLength) - 1);
        ushort clearMask = (ushort)~(valueMask << bitOffset);

        ushort preparedValue = (ushort)((value & valueMask) << bitOffset);

        for (int i = 0; i < 2 && byteIndex < ByteLength; i++)
        {
            ref byte byteRef = ref Value[byteIndex++];
            int shiftAmount = Helpers.BitsPerByte * i;
            byteRef &= (byte)(clearMask >> shiftAmount);
            byteRef |= (byte)(preparedValue >> shiftAmount);
        }
    }
}
