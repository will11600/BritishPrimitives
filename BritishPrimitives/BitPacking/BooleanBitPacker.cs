namespace BritishPrimitives.BitPacking;

internal static class BooleanBitPacker
{
    public const int SizeInBits = 1;

    public static bool TryPackBit(this ref readonly BitWriter writer, ref int position, bool value)
    {
        if (Helpers.HasAvailableBits(in writer, position, SizeInBits))
        {
            writer.WriteByte(position, value ? byte.MaxValue : byte.MinValue, 1);
            position += SizeInBits;
            return true;
        }

        return false;
    }

    public static Bit UnpackBit(this ref readonly BitReader reader, ref int position)
    {
        if (Helpers.HasAvailableBits(in reader, position, SizeInBits))
        {
            Bit result = (Bit)reader.ReadByte(position, SizeInBits);
            position += SizeInBits;
            return result;
        }

        return Bit.Error;
    }
}
