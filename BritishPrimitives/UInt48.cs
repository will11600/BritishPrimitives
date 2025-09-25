using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BritishPrimitives;

[StructLayout(LayoutKind.Explicit, Size = 6)]
internal readonly struct UInt48 : IEquatable<UInt48>
{
    [FieldOffset(0)]
    private readonly uint _lo;
    [FieldOffset(4)]
    private readonly ushort _hi;

    private UInt48(ulong value)
    {
        _lo = (uint)value;
        _hi = (ushort)(value >> 32);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt48 operator <<(UInt48 left, int right)
    {
        return new UInt48((ulong)left << right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt48 operator >>(UInt48 left, int right)
    {
        return new UInt48((ulong)left >> right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt48 operator &(UInt48 left, int right)
    {
        return new UInt48((ulong)left & (uint)right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt48 operator |(UInt48 left, int right)
    {
        return new UInt48((ulong)left | (uint)right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(UInt48 left, UInt48 right)
    {
        return left._lo == right._lo && left._hi == right._hi;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(UInt48 left, UInt48 right)
    {
        return left._lo != right._lo || left._hi != right._hi;
    }

    public override bool Equals(object? obj)
    {
        return obj is UInt48 other && Equals(other);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(UInt48 other)
    {
        return _lo == other._lo && _hi == other._hi;
    }

    public override int GetHashCode()
    {
        return (int)_lo ^ _hi;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator ulong(UInt48 value)
    {
        return ((ulong)value._hi << 32) | value._lo;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator UInt48(ulong value)
    {
        return new UInt48(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator int(UInt48 value)
    {
        return (int)value._lo;
    }
}