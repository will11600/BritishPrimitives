using BritishPrimitives.BitPacking;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BritishPrimitives;

[StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
public unsafe struct InwardPostalCode : IPrimitive<InwardPostalCode>
{
    internal const int SizeInBytes = 3;

    private static readonly SearchValues<char> _invalidLetters = SearchValues.Create("CIKMOVcikmov");

    [FieldOffset(0)]
    private fixed byte _value[SizeInBytes];

    /// <inheritdoc/>
    public static int MaxLength { get; } = 3;

    internal InwardPostalCode(ReadOnlySpan<char> chars)
    {
        int position = 0;

        fixed (byte* ptr = _value)
        {
            BitWriter writer = BitWriter.Create(ptr, SizeInBytes);
            writer.PackLettersAndNumbers(ref position, chars);
        }
    }

    public static InwardPostalCode Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out InwardPostalCode result))
        {
            return result;
        }

        throw new FormatException(Helpers.FormatExceptionMessage);
    }

    public static InwardPostalCode Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s.AsSpan(), provider, out InwardPostalCode result))
        {
            return result;
        }

        throw new FormatException(Helpers.FormatExceptionMessage);
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out InwardPostalCode result)
    {
        int offset = Character.SkipWhitespace(s);
        int length;

        var payload = s[offset..];

        result = new InwardPostalCode();
        fixed (byte* ptr = result._value)
        {
            BitWriter writer = BitWriter.Create(ptr, SizeInBytes);
            int position = 0;

            if (!writer.TryPackAlphanumericLetter(ref position, payload[0]))
            {
                return Helpers.FalseOutDefault(out result);
            }

            for (length = 1; length < payload.Length; length++)
            {
                ref readonly char c = ref payload[length];
                if (!_invalidLetters.Contains(c) && writer.TryPackAlphanumericLetter(ref position, c))
                {
                    continue;
                }

                return false;
            }
        }

        return length == MaxLength;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out InwardPostalCode result)
    {
        return TryParse(s.AsSpan(), provider, out result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(InwardPostalCode other)
    {
        return this == other;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is InwardPostalCode other && this == other;
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        Span<char> buffer = stackalloc char[MaxLength];
        if (TryFormat(buffer, out int charsWritten, format.AsSpan(), formatProvider))
        {
            return buffer[..charsWritten].ToString();
        }

        throw new FormatException(Helpers.FormatExceptionMessage);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        return ToString(null, null);
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        int position = 0;

        fixed (byte* pValue = _value)
        {
            BitReader reader = BitReader.Create(pValue, SizeInBytes);
            charsWritten = reader.UnpackLettersAndNumbers(ref position, destination);
        }

        return charsWritten == MaxLength;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(InwardPostalCode left, InwardPostalCode right)
    {
        return Helpers.SequenceEquals(left._value, right._value, SizeInBytes);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(InwardPostalCode left, InwardPostalCode right)
    {
        return !Helpers.SequenceEquals(left._value, right._value, SizeInBytes);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator InwardPostalCode(ulong value)
    {
        InwardPostalCode result = new();
        Helpers.SpreadBytes(value, result._value, SizeInBytes);
        return result;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator ulong(InwardPostalCode value)
    {
        return Helpers.ConcatenateBytes(value._value, SizeInBytes);
    }

    public override int GetHashCode()
    {
        fixed (byte* ptr = _value)
        {
            return Helpers.BuildHashCode(ptr, SizeInBytes);
        }
    }
}
