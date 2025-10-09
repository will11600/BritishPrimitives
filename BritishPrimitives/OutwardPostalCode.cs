using BritishPrimitives.BitPacking;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BritishPrimitives;

[StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
public unsafe struct OutwardPostalCode : IPrimitive<OutwardPostalCode>
{
    internal const int SizeInBytes = 3;

    private static readonly SearchValues<char> _invalidFirstLetters = SearchValues.Create("IJQVXZijqvxz");
    private static readonly SearchValues<char> _invalidSecondLetters = SearchValues.Create("CIKMOVQUZcikmovquz");

    [FieldOffset(0)]
    private fixed byte _value[SizeInBytes];

    /// <inheritdoc/>
    public static int MaxLength { get; } = 4;

    /// <summary>
    /// The minimum length in characters of the <see langword="string"/> representation of <see cref="OutwardPostalCode"/>.
    /// </summary>
    public static int MinLength { get; } = 2;

    internal OutwardPostalCode(ReadOnlySpan<char> chars)
    {
        int position = 0;

        fixed (byte* ptr = _value)
        {
            BitWriter writer = BitWriter.Create(ptr, SizeInBytes);
            writer.PackLettersAndNumbers(ref position, chars);
        }
    }

    public static OutwardPostalCode Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out OutwardPostalCode result))
        {
            return result;
        }

        throw new FormatException(Helpers.FormatExceptionMessage);
    }

    public static OutwardPostalCode Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s.AsSpan(), provider, out OutwardPostalCode result))
        {
            return result;
        }

        throw new FormatException(Helpers.FormatExceptionMessage);
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out OutwardPostalCode result)
    {
        int offset = Character.SkipWhitespace(s);
        var payload = s[offset..];

        OutwardPostalCode outwardCode = new();

        BitWriter writer = BitWriter.Create(outwardCode._value, SizeInBytes);
        int position = 0;

        return !_invalidFirstLetters.Contains(payload[0]) && payload.Length switch
        {
            2 => TryWritePattern_A9(in writer, payload, ref position),
            3 => TryWritePattern_A99_AA9_ANA(in writer, payload, ref position),
            4 => TryWritePattern_AANN_AANA(writer, payload, ref position),
            _ => false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryWritePattern_A9(ref readonly BitWriter writer, ReadOnlySpan<char> payload, ref int position)
    {
        return writer.TryPackAlphanumericLetter(ref position, payload[0]) && writer.TryPackAlphanumericDigit(ref position, payload[1]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryWritePattern_A99_AA9_ANA(ref readonly BitWriter writer, ReadOnlySpan<char> payload, ref int position)
    {
        return writer.TryPackAlphanumeric(ref position, payload[0]) 
            && (writer.TryPackAlphanumericDigit(ref position, payload[1]) 
            || TryWritePattern_AA9_ANA(writer, payload[1], payload[2], ref position));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryWritePattern_AA9_ANA(BitWriter writer, char c1, char c2, ref int position)
    {
        return writer.TryPackAlphanumericLetter(ref position, c1) && !_invalidSecondLetters.Contains(c1) && writer.TryPackAlphanumericDigit(ref position, c2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryWritePattern_AANN_AANA(BitWriter writer, ReadOnlySpan<char> payload, ref int position)
    {
        return writer.PackAlphanumericLetters(ref position, payload[..2]) == 2 
            && writer.TryPackAlphanumericDigit(ref position, payload[3])
            && writer.TryPackAlphanumeric(ref position, payload[4]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out OutwardPostalCode result)
    {
        return TryParse(s.AsSpan(), provider, out result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(OutwardPostalCode other)
    {
        return this == other;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is OutwardPostalCode other && this == other;
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

        return charsWritten >= MinLength && charsWritten <= MaxLength;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(OutwardPostalCode left, OutwardPostalCode right)
    {
        return Helpers.SequenceEquals(left._value, right._value, SizeInBytes);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(OutwardPostalCode left, OutwardPostalCode right)
    {
        return !Helpers.SequenceEquals(left._value, right._value, SizeInBytes);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator OutwardPostalCode(ulong value)
    {
        OutwardPostalCode result = new();
        Helpers.SpreadBytes(value, result._value, SizeInBytes);
        return result;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator ulong(OutwardPostalCode value)
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