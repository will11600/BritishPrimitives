using System.Runtime.CompilerServices;

namespace BritishPrimitives.BitPacking;

internal unsafe readonly ref struct BitWriter
{
    public required byte* Value { get; init; }

    public required int ByteLength { get; init; }

    public required long BitLength { get; init; }

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitWriter Create(byte* ptr, int byteLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(byteLength, nameof(byteLength));
        return new BitWriter()
        {
            Value = ptr,
            ByteLength = byteLength,
            BitLength = byteLength * Helpers.BitsPerByte
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitWriter Create<T>(ref T value) where T : unmanaged
    {
        int byteLength = sizeof(T);
        return new BitWriter()
        {
            Value = (byte*)Unsafe.AsPointer(ref value),
            ByteLength = byteLength,
            BitLength = byteLength * Helpers.BitsPerByte
        };
    }
}
