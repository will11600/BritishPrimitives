using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static BritishPrimitives.CharUtils;

namespace BritishPrimitives;

/// <summary>
/// Represents a UK National Insurance Number (NINo), a unique reference number
/// used in the administration of the UK social security and tax systems.
/// </summary>
/// <remarks>
/// A National Insurance Number consists of two prefix letters, six digits, and a suffix letter.
/// It is stored internally in a compact, read-only form.
/// </remarks>
[StructLayout(LayoutKind.Explicit, Size = 5)]
public readonly struct NationalInsuranceNumber : IPrimitive<NationalInsuranceNumber>
{
    private readonly struct UnpackedNationalInsuranceNumber(uint lo, byte hi)
    {
        const uint DigitMask = (1U << DigitBits) - 1;
        const uint LetterMask = (1U << LetterBits) - 1;

        public readonly uint digits = hi & DigitMask;
        public readonly char prefix1 = (char)(((lo >> DigitBits) & LetterMask) + UppercaseA);
        public readonly char prefix2 = (char)(((lo >> (DigitBits + LetterBits)) & LetterMask) + UppercaseA);
        public readonly char suffix = (char)(hi + UppercaseA);
    }

    private const string IllegalPrefixChars = "DFIQUV";
    private const string IllegalPrefixes = "BGGBKNNKNTTNZZ";

    private const string FormatSpecifiers = "GS";
    private static char FormatGeneral => FormatSpecifiers[0];
    private static char FormatSpaced => FormatSpecifiers[1];

    /// <inheritdoc/>
    public static int MaxLength => SpacedNiStringLength;

    private const int NiStringLength = 9;
    private const int SpacedNiStringLength = 13;

    private const int PrefixLength = 2;
    private const int SuffixOffset = 6;

    private const int DigitBits = 20;
    private const string DigitFormat = "D6";

    [FieldOffset(0)]
    private readonly uint _lo;

    [FieldOffset(4)]
    private readonly byte _hi;
    
    private NationalInsuranceNumber(uint lo, byte hi)
    {
        _lo = lo;
        _hi = hi;
    }

    /// <summary>
    /// Converts the span representation of a National Insurance Number to its <see cref="NationalInsuranceNumber"/> equivalent.
    /// </summary>
    /// <param name="s">A span containing the National Insurance Number to convert.</param>
    /// <param name="provider">An optional object that supplies culture-specific formatting information. This is currently unused.</param>
    /// <returns>A <see cref="NationalInsuranceNumber"/> equivalent to the number contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is not in a valid National Insurance Number format.
    /// </exception>
    public static NationalInsuranceNumber Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null)
    {
        if (TryParse(s, provider, out NationalInsuranceNumber ni))
        {
            return ni;
        }

        throw new FormatException("Invalid national insurance number format.");
    }

    /// <summary>
    /// Converts the string representation of a National Insurance Number to its <see cref="NationalInsuranceNumber"/> equivalent.
    /// </summary>
    /// <param name="s">A string containing the National Insurance Number to convert.</param>
    /// <param name="provider">An optional object that supplies culture-specific formatting information. This is currently unused.</param>
    /// <returns>A <see cref="NationalInsuranceNumber"/> equivalent to the number contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is <see langword="null"/> or is not in a valid National Insurance Number format.
    /// </exception>
    public static NationalInsuranceNumber Parse(string s, IFormatProvider? provider = null)
    {
        if (s is not null && TryParse(s.AsSpan(), provider, out NationalInsuranceNumber ni))
        {
            return ni;
        }

        throw new FormatException("Invalid national insurance number format.");
    }

    /// <summary>
    /// Tries to convert the span representation of a National Insurance Number to its <see cref="NationalInsuranceNumber"/> equivalent.
    /// </summary>
    /// <param name="s">A span containing the National Insurance Number to convert.</param>
    /// <param name="provider">An optional object that supplies culture-specific formatting information. This is currently unused.</param>
    /// <param name="result">
    /// When this method returns, contains the <see cref="NationalInsuranceNumber"/> equivalent to the number contained in <paramref name="s"/>,
    /// if the conversion succeeded, or the default value if the conversion failed.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out NationalInsuranceNumber result)
    {
        Span<char> sanitized = stackalloc char[NiStringLength];
        if (!TryParseAlphanumericUpperInvariant(s, sanitized, out int charsWritten) || charsWritten != NiStringLength)
        {
            return FalseOutDefault(out result);
        }

        Span<char> prefix = sanitized[0..PrefixLength];
        for (int i = 0; i < IllegalPrefixes.Length; i += PrefixLength)
        {
            if (prefix.SequenceEqual(IllegalPrefixes.AsSpan(i, PrefixLength)))
            {
                return FalseOutDefault(out result);
            }
        }

        if (!uint.TryParse(sanitized[PrefixLength..SuffixOffset], out uint lo) || lo > 999999U)
        {
            return FalseOutDefault(out result);
        }

        for (int i = 0; i < PrefixLength; i++)
        {
            ref readonly char c = ref prefix[i];

            if (IsValidPrefixLetter(c, i))
            {
                lo |= (uint)UppercaseEncode(sanitized[i]) << (DigitBits + (LetterBits * i));
                continue;
            }

            return FalseOutDefault(out result);
        }

        byte hi = UppercaseEncode(sanitized[^1]);

        result = new NationalInsuranceNumber(lo, hi);
        return true;
    }

    /// <summary>
    /// Tries to convert the string representation of a National Insurance Number to its <see cref="NationalInsuranceNumber"/> equivalent.
    /// </summary>
    /// <param name="s">A string containing the National Insurance Number to convert.</param>
    /// <param name="provider">An optional object that supplies culture-specific formatting information. This is currently unused.</param>
    /// <param name="result">
    /// When this method returns, contains the <see cref="NationalInsuranceNumber"/> equivalent to the number contained in <paramref name="s"/>,
    /// if the conversion succeeded, or the default value if the conversion failed.
    /// </param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out NationalInsuranceNumber result)
    {
        if (s is null)
        {
            return FalseOutDefault(out result);
        }

        return TryParse(s.AsSpan(), provider, out result);
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(NationalInsuranceNumber other)
    {
        return _hi == other._hi && _lo == other._lo;
    }

    /// <summary>
    /// Converts the value of the current <see cref="NationalInsuranceNumber"/> object to its equivalent string representation, 
    /// using the specified format and culture-specific format information.
    /// </summary>
    /// <param name="format">A format string. Supported formats: 'G' (default, e.g., "QQ123456C"), 'S' (spaced, e.g., "QQ 12 34 56 C").</param>
    /// <param name="formatProvider">An optional object that supplies culture-specific formatting information.</param>
    /// <returns>The string representation of the current <see cref="NationalInsuranceNumber"/> object, formatted as specified.</returns>
    /// <exception cref="FormatException">The format string is invalid.</exception>
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        Span<char> s = stackalloc char[NiStringLength];
        ReadOnlySpan<char> formatChars = format is null ? [] : format.AsSpan();
        if (TryFormat(s, out int charsWritten, formatChars, null))
        {
            return s[..charsWritten].ToString();
        }

        throw new FormatException("The format is invalid.");
    }

    /// <summary>
    /// Returns the string representation of this <see cref="NationalInsuranceNumber"/> using the general ('G') format.
    /// </summary>
    /// <returns>A string representing this National Insurance Number, e.g., "QQ123456C".</returns>
    public override string ToString()
    {
        return ToString(null, null);
    }

    /// <summary>
    /// Tries to format the current National Insurance Number instance into the provided span of characters.
    /// </summary>
    /// <param name="destination">The span in which to write the formatted value.</param>
    /// <param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination"/>.</param>
    /// <param name="format">
    /// A format string. Supported formats:
    /// 'G' (default) - Formats as a single block, e.g., "QQ123456C".
    /// 'S' - Formats with spaces, e.g., "QQ 12 34 56 C".
    /// </param>
    /// <param name="provider">An optional object that supplies culture-specific formatting information. This is currently unused.</param>
    /// <returns><see langword="true"/> if the formatting was successful; otherwise, <see langword="false"/>.</returns>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider = null)
    {
        char specifier = format.IsEmpty ? FormatGeneral : char.ToUpperInvariant(format[0]);

        if (format.Length > 1 || !FormatSpecifiers.Contains(specifier))
        {
            charsWritten = 0;
            return false;
        }

        UnpackedNationalInsuranceNumber unpacked = new(_lo, _hi);
        if (specifier == FormatSpaced)
        {
            return TryFormatSpaced(in unpacked, destination, out charsWritten, provider);
        }

        return TryFormatGeneral(in unpacked, destination, out charsWritten, provider);
    }

    /// <summary>
    /// Indicates whether this instance and a specified object are equal.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="NationalInsuranceNumber"/> that equals the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is NationalInsuranceNumber ni && Equals(ni);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(_lo, _hi);
    }

    /// <summary>
    /// Determines whether two specified <see cref="NationalInsuranceNumber"/> objects have the same value.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(NationalInsuranceNumber left, NationalInsuranceNumber right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two specified <see cref="NationalInsuranceNumber"/> objects have different values.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/> is not equal to <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(NationalInsuranceNumber left, NationalInsuranceNumber right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Explicitly converts a <see cref="NationalInsuranceNumber"/> to its 64-bit unsigned integer representation.
    /// </summary>
    /// <param name="ni">The national insurance number to convert.</param>
    /// <returns>The <see langword="ulong"/> representation of the national insurance number.</returns>
    public static explicit operator ulong(NationalInsuranceNumber ni)
    {
        return (ulong)ni._hi << (sizeof(uint) * BitsPerByte) | ni._lo;
    }

    /// <summary>
    /// Explicitly converts a 64-bit unsigned integer to its <see cref="NationalInsuranceNumber"/> representation.
    /// </summary>
    /// <param name="value">The <see langword="ulong"/> value to convert.</param>
    /// <returns>The <see cref="NationalInsuranceNumber"/> representation of the value.</returns>
    public static explicit operator NationalInsuranceNumber(ulong value)
    {
        return new NationalInsuranceNumber((uint)value, (byte)(value >> (sizeof(uint) * BitsPerByte)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryFormatSpaced(ref readonly UnpackedNationalInsuranceNumber unpacked, Span<char> destination, out int charsWritten, IFormatProvider? provider)
    {
        Span<char> digitChars = stackalloc char[SuffixOffset];
        if (!unpacked.digits.TryFormat(digitChars, out _, DigitFormat, provider))
        {
            return FalseOutDefault(out charsWritten);
        }

        destination[00] = unpacked.prefix1;
        destination[01] = unpacked.prefix2;
        destination[02] = Whitespace;
        destination[03] = digitChars[0];
        destination[04] = digitChars[1];
        destination[05] = Whitespace;
        destination[06] = digitChars[2];
        destination[07] = digitChars[3];
        destination[08] = Whitespace;
        destination[09] = digitChars[4];
        destination[10] = digitChars[5];
        destination[11] = Whitespace;
        destination[12] = unpacked.suffix;

        charsWritten = SpacedNiStringLength;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryFormatGeneral(ref readonly UnpackedNationalInsuranceNumber unpacked, Span<char> destination, out int charsWritten, IFormatProvider? provider)
    {
        destination[0] = unpacked.prefix1;
        destination[1] = unpacked.prefix2;

        if (!unpacked.digits.TryFormat(destination.Slice(PrefixLength, SuffixOffset), out _, DigitFormat, provider))
        {
            return FalseOutDefault(out charsWritten);
        }

        destination[NiStringLength - 1] = unpacked.suffix;

        charsWritten = NiStringLength;
        return true;
    }

    private static bool IsValidPrefixLetter(char c, int index)
    {
        if (c is < UppercaseA or > UppercaseZ)
        {
            return false;
        }

        switch (index)
        {
            case 1:
                if (c == 'O')
                {
                    return false;
                }
                goto case 0;
            default:
            case 0:
                return !IllegalPrefixChars.Contains(c);
        }
    }
}
