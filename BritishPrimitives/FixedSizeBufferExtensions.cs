using System.Runtime.CompilerServices;
using static BritishPrimitives.CharUtils;

namespace BritishPrimitives;

internal static unsafe class FixedSizeBufferExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SequenceEquals(byte* a, byte* b, int length)
    {
        ReadOnlySpan<byte> aSpan = new(a, length);
        ReadOnlySpan<byte> bSpan = new(b, length);
        return aSpan.SequenceEqual(bSpan);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ConcatenateBytes(byte* ptr, int length)
    {
        ulong result = default;
        for (int i = 0; i < length; i++)
        {
            result |= (ulong)ptr[i] << (BitsPerByte * i);
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SpreadBytes(ulong number, byte* ptr, int length)
    {
        for (int i = 0; i < length; i++)
        {
            ptr[i] = (byte)(number >> (BitsPerByte * i));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BuildHashCode(byte* ptr, int length)
    {
        HashCode hashCode = new();
        for (int i = 0; i < length; i++)
        {
            hashCode.Add(ptr[i]);
        }
        return hashCode.ToHashCode();
    }
}
