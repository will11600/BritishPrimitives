using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace BritishPrimitives.BitPacking;

internal static class NumericBitPacker
{
    public const int SizeInBits = sizeof(ulong) * Helpers.BitsPerByte;

    public static bool TryUnpackUInt64(this ref readonly BitReader reader, ref int position, out ulong value, ulong max = ulong.MaxValue)
    {
        value = default;

        int remainingBits = BitOperations.Log2(max);

        if (!reader.CanRead(position, remainingBits))
        {
            return false;
        }

        while (remainingBits > 0)
        {
            int chunkLength = Math.Min(remainingBits, Helpers.BitsPerByte);
            byte chunk = reader.ReadByte(position, chunkLength);

            value <<= chunkLength;
            value |= chunk;

            position += chunkLength;
            remainingBits -= chunkLength;
        }

        return true;
    }

    public static bool TryPackUInt64(this ref readonly BitWriter writer, ref int position, ulong value, ulong max = ulong.MaxValue)
    {
        int maxBits = BitOperations.Log2(max);
        int bitsToWrite = Math.Min(maxBits, SizeInBits - BitOperations.LeadingZeroCount(value));

        if (!writer.CanWrite(position, bitsToWrite))
        {
            return false;
        }

        int remainingBits = bitsToWrite;

        while (remainingBits > 0)
        {
            int chunkLength = Math.Min(remainingBits, Helpers.BitsPerByte);

            int shiftAmount = remainingBits - chunkLength;
            byte chunkValue = (byte)(value >> shiftAmount);

            writer.WriteByte(position, chunkValue, chunkLength);

            position += chunkLength;
            remainingBits -= chunkLength;
        }

        position += maxBits - bitsToWrite;

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryPackNumericString(this ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> chars)
    {
        ulong max = Helpers.Max((uint)chars.Length);
        return max < uint.MaxValue 
            && uint.TryParse(chars, NumberStyles.None, CultureInfo.InvariantCulture, out uint result) 
            && writer.TryPackUInt64(ref position, result, (uint)max);
    }
}
