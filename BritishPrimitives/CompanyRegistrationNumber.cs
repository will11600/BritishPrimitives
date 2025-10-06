using BritishPrimitives.BitPacking;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BritishPrimitives;

/// <summary>
/// Represents a UK Company Registration Number (CRN).
/// </summary>
/// Internal Bit Layout (43 bits):
/// --------------------------------------------------------------------------------------------
/// | Bit 1       | Bits 2-22 (alphanumeric) | Bits 2-29 (numeric) | Bits 23-43 (alphanumeric) |
/// |-------------|--------------------------|---------------------|---------------------------|
/// | Prefix Type | Prefix                   | Main Number         | Main Number               |
/// | (1 bit)     | (10 bits)                | (27 bits)           | (20 bits)                 |
/// --------------------------------------------------------------------------------------------
[StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
public unsafe struct CompanyRegistrationNumber : IPrimitive<CompanyRegistrationNumber>
{
    private const int SizeInBytes = 6;

    private const int PrefixLength = 2;

    [FieldOffset(0)]
    private fixed byte _value[SizeInBytes];

    /// <inheritdoc/>
    public static int MaxLength { get; } = 8;

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

        throw new FormatException("Input string was not in a correct format.");
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

        if (s.IsEmpty || s.Length < MaxLength)
        {
            return false;
        }

        fixed (byte* ptr = result._value)
        {
            BitWriter writer = BitWriter.Create(ptr, SizeInBytes);
            int position = 0;

            if (TryWriteNumeralTypePrefix(in writer, s, ref position, out int offset) || TryWriteLetterTypePrefix(in writer, s, ref position, out offset))
            {
                return writer.TryPackNumbers(ref position, s[offset..MaxLength]);
            }
        }

        return false;
    }

    private static bool TryWriteLetterTypePrefix(ref readonly BitWriter writer, ReadOnlySpan<char> s, ref int position, out int offset)
    {
        if (TryWritePrefixType(in writer, s, ref position, Character.IsLetter, true, out Range prefixRange))
        {
            offset = Helpers.CalculateOffset(s, prefixRange);
            return writer.PackLetters(ref position, s[prefixRange]) == PrefixLength;
        }

        return Helpers.FalseOutDefault(out offset);
    }

    private static bool TryWriteNumeralTypePrefix(ref readonly BitWriter writer, ReadOnlySpan<char> s, ref int position, out int offset)
    {
        if (TryWritePrefixType(in writer, s, ref position, Character.IsDigit, false, out Range prefixRange))
        {
            offset = Helpers.CalculateOffset(s, prefixRange);
            return true;
        }

        return Helpers.FalseOutDefault(out offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryWritePrefixType(ref readonly BitWriter writer, ReadOnlySpan<char> s, ref int position, Func<char, bool> predicate, bool value, out Range prefixRange)
    {
        return Character.ContiguousSequenceWithoutWhitespace(s, predicate, PrefixLength, PrefixLength, out prefixRange) && writer.TryWriteBit(ref position, value);
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

        throw new FormatException("Input string was not in a correct format.");
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
        if (destination.Length < MaxLength)
        {
            return Helpers.FalseOutDefault(out charsWritten);
        }

        fixed (byte* ptr = _value)
        {
            BitReader reader = BitReader.Create(ptr, SizeInBytes);

            charsWritten = 0;
            int position = 0;

            if (TryFormatPrefix(in reader, ref position, destination, ref charsWritten))
            {
                charsWritten += reader.UnpackNumbers(ref position, destination[charsWritten..MaxLength]);
                return charsWritten == MaxLength;
            }

            return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryFormatPrefix(ref readonly BitReader reader, ref int position, Span<char> destination, ref int charsWritten) => reader.ReadBit(ref position) switch
    {
        Bit.False => true,
        Bit.True => (charsWritten += reader.UnpackLetters(ref position, destination[..PrefixLength])) == PrefixLength,
        _ => false
    };

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

    /// <summary>
    /// Determines whether two specified <see cref="CompanyRegistrationNumber"/> objects have the same value.
    /// </summary>
    /// <param name="left">The first <see cref="CompanyRegistrationNumber"/> to compare.</param>
    /// <param name="right">The second <see cref="CompanyRegistrationNumber"/> to compare.</param>
    /// <returns><see langword="true"/> if the value of <paramref name="left"/> is the same as the value of <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(CompanyRegistrationNumber left, CompanyRegistrationNumber right)
    {
        return Helpers.SequenceEquals(left._value, right._value, SizeInBytes);
    }

    /// <summary>
    /// Determines whether two specified <see cref="CompanyRegistrationNumber"/> objects have different values.
    /// </summary>
    /// <param name="left">The first <see cref="CompanyRegistrationNumber"/> to compare.</param>
    /// <param name="right">The second <see cref="CompanyRegistrationNumber"/> to compare.</param>
    /// <returns><see langword="true"/> if the value of <paramref name="left"/> is different from the value of <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
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