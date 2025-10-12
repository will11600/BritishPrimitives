using BritishPrimitives.Text;

namespace BritishPrimitives.BitPacking;

internal static class CharacterBitPacker
{
    public static int UnpackCharacters(this ref readonly BitReader reader, ref int position, Span<char> chars, CharacterTranscoder transcoder)
    {
        int count = 0;

        int length = Helpers.ClampAvailableBits(in reader, position, transcoder.sizeInBits, chars.Length);
        for (; count < length && TryUnpackCharacter(in reader, ref position, transcoder, out char result); count++)
        {
            chars[count] = result;
            position += transcoder.sizeInBits;
        }

        return count;
    }

    public static bool TryUnpackCharacter(this ref readonly BitReader reader, ref int position, CharacterTranscoder transcoder, out char result)
    {
        int charByte = reader.ReadByte(position, transcoder.sizeInBits);
        return transcoder.TryDecode(charByte - 1, out result);
    }

    public static int PackCharacters(this ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> chars, CharacterTranscoder transcoder)
    {
        int count = 0;

        int length = Helpers.ClampAvailableBits(in writer, position, transcoder.sizeInBits, chars.Length);
        for (; count < length && transcoder.TryEncode(chars[count], out int normalizedChar); count++)
        {
            writer.WriteByte(position, (byte)(normalizedChar + 1), transcoder.sizeInBits);
            position += transcoder.sizeInBits;
        }

        return count;
    }

    public static bool TryPackCharacter(this ref readonly BitWriter writer, ref int position, char value, CharacterTranscoder transcoder)
    {
        if (Helpers.HasAvailableBits(in writer, position, transcoder.sizeInBits) && transcoder.TryEncode(value, out int normalizedChar))
        {
            writer.WriteByte(position, (byte)(normalizedChar + 1), transcoder.sizeInBits);
            position += transcoder.sizeInBits;
            return true;
        }

        return false;
    }
}