using BritishPrimitives.BitPacking;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BritishPrimitives;

/// <summary>
/// Represents a UK Company Registration Number (CRN).
/// </summary>
/// Internal Bit Layout (42 bits):
/// ------------------------------------------
/// | Bit 1       | Bits 2-14 | Bits 15-42   |
/// |-------------|-----------|--------------|
/// | Type Flag   | Prefix    | Main Number  |
/// | (1 bit)     | (12 bits) | (20-27 bits) |
/// ------------------------------------------
[StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
public unsafe struct CompanyRegistrationNumber : IPrimitive<CompanyRegistrationNumber>
{
    private const int RequiredLength = 8;
    private const int SizeInBytes = 6;
    private const int PrefixLength = 2;
    private const int TypeFlagBitLength = 1;
    private const int PrefixBitLength = PrefixLength * AlphanumericBitPacker.SizeInBits;

    [FieldOffset(0)]
    private fixed byte _value[SizeInBytes];

    /// <inheritdoc/>
    public static int MaxLength { get; } = RequiredLength;

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <see langword="false"/>.</returns>
    public readonly bool Equals(CompanyRegistrationNumber other)
    {
        return this == other;
    }

    /// <summary>
    /// Indicates whether this instance and a specified object are equal.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, <see langword="false"/>.</returns>
    public override readonly bool Equals(object? obj)
    {
        return obj is CompanyRegistrationNumber other && Equals(other);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override readonly int GetHashCode()
    {
        fixed (byte* ptr = _value)
        {
            return Helpers.BuildHashCode(ptr, SizeInBytes);
        }
    }

    /// <summary>
    /// Converts the span representation of a UK Company Registration Number to its <see cref="CompanyRegistrationNumber"/> equivalent.
    /// </summary>
    /// <param name="s">A read-only span of characters containing the Company Registration Number to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information (currently ignored).</param>
    /// <returns>A <see cref="CompanyRegistrationNumber"/> equivalent to the number contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is not in a correct format, or is <see langword="null"/> or empty, or its length exceeds <see cref="MaxLength"/>.
    /// </exception>
    public static CompanyRegistrationNumber Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out CompanyRegistrationNumber result))
        {
            return result;
        }

        throw new FormatException(Helpers.FormatExceptionMessage);
    }

    /// <summary>
    /// Tries to convert the read-only span of characters into a <see cref="CompanyRegistrationNumber"/> value.
    /// </summary>
    /// <param name="s">The read-only span of characters to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information (currently ignored).</param>
    /// <param name="result">When this method returns, contains the <see cref="CompanyRegistrationNumber"/> value equivalent to the number contained in <paramref name="s"/>, if the conversion succeeded, or the default value if the conversion failed.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out CompanyRegistrationNumber result)
    {
        result = new CompanyRegistrationNumber();

        var payload = s.Trim();

        if (payload.Length != RequiredLength)
        {
            return false;
        }

        fixed (byte* ptr = result._value)
        {
            BitWriter writer = BitWriter.Create(ptr, SizeInBytes);

            int position = TypeFlagBitLength;
            bool isLetterPrefix = false;
            int mainNumberOffset = 0;

            switch (writer.PackLetters(ref position, payload[..PrefixLength]))
            {
                case PrefixLength:
                    isLetterPrefix = true;
                    mainNumberOffset = PrefixBitLength;
                    break;
                case 0:
                    break;
                default:
                    return false;
            }

            position = 0;
            bool prefixWritten = writer.TryPackBit(ref position, isLetterPrefix);
            position += PrefixBitLength;

            return prefixWritten && writer.TryPackInteger(ref position, payload[mainNumberOffset..]);
        }
    }

    /// <summary>
    /// Converts the string representation of a UK Company Registration Number to its <see cref="CompanyRegistrationNumber"/> equivalent.
    /// </summary>
    /// <param name="s">A string containing the Company Registration Number to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information (currently ignored).</param>
    /// <returns>A <see cref="CompanyRegistrationNumber"/> equivalent to the number contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is not in a correct format, or is <see langword="null"/> or empty, or its length exceeds <see cref="MaxLength"/>.
    /// </exception>
    public static CompanyRegistrationNumber Parse(string s, IFormatProvider? provider = null)
    {
        if (TryParse(s, provider, out CompanyRegistrationNumber result))
        {
            return result;
        }

        throw new FormatException(Helpers.FormatExceptionMessage);
    }

    /// <summary>
    /// Tries to convert the string representation of a UK Company Registration Number to its <see cref="CompanyRegistrationNumber"/> equivalent.
    /// </summary>
    /// <param name="s">A string containing the Company Registration Number to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information (currently ignored).</param>
    /// <param name="result">When this method returns, contains the <see cref="CompanyRegistrationNumber"/> value equivalent to the number contained in <paramref name="s"/>, if the conversion succeeded, or the default value if the conversion failed.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out CompanyRegistrationNumber result)
    {
        return TryParse(s is null ? [] : s.AsSpan(), provider, out result);
    }

    /// <summary>
    /// Tries to format the value of the current instance into the provided span of characters.
    /// </summary>
    /// <param name="destination">The span in which to write the formatted Company Registration Number.</param>
    /// <param name="charsWritten">When this method returns, the number of characters that were written in <paramref name="destination"/>.</param>
    /// <param name="format">A read-only span containing the format to use (currently ignored).</param>
    /// <param name="provider">An object that supplies culture-specific formatting information (currently ignored).</param>
    /// <returns><see langword="true"/> if the formatting was successful; otherwise, <see langword="false"/>.</returns>
    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider = null)
    {
        if (destination.Length < RequiredLength)
        {
            return Helpers.FalseOutDefault(out charsWritten);
        }

        fixed (byte* ptr = _value)
        {
            BitReader reader = BitReader.Create(ptr, SizeInBytes);

            charsWritten = 0;
            int position = 0;

            switch (reader.UnpackBit(ref position))
            {
                case Bit.True:
                    charsWritten += reader.UnpackAlphanumeric(ref position, destination[..PrefixLength]);
                    break;
                case Bit.False:
                    position += PrefixBitLength;
                    break;
                default:
                    return false;
            }

            charsWritten += reader.UnpackInteger(ref position, destination[charsWritten..RequiredLength]);
            return charsWritten == MaxLength;
        }
    }

    /// <summary>
    /// Converts the value of the current <see cref="CompanyRegistrationNumber"/> object to its equivalent string representation.
    /// </summary>
    /// <param name="format">A format string (currently ignored).</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information (currently ignored).</param>
    /// <returns>The string representation of the current <see cref="CompanyRegistrationNumber"/> value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        Span<char> span = stackalloc char[MaxLength];
        if (TryFormat(span, out int charsWritten, null, null))
        {
            return span[..charsWritten].ToString();
        }

        return string.Empty;
    }

    /// <summary>
    /// Returns the string representation of the Company Registration Number.
    /// </summary>
    /// <returns>A string that represents the current Company Registration Number.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly string ToString()
    {
        return ToString(null, null);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(CompanyRegistrationNumber left, CompanyRegistrationNumber right)
    {
        return Helpers.SequenceEquals(left._value, right._value, SizeInBytes);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(CompanyRegistrationNumber left, CompanyRegistrationNumber right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator ulong(CompanyRegistrationNumber value)
    {
        return Helpers.ConcatenateBytes(value._value, SizeInBytes);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator CompanyRegistrationNumber(ulong value)
    {
        CompanyRegistrationNumber result = new();
        Helpers.SpreadBytes(value, result._value, SizeInBytes);
        return result;
    }
}