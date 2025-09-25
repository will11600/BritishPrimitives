using System.Diagnostics.CodeAnalysis;

namespace BritishPrimitives;

/// <summary>
/// Represents a UK Company Registration Number (CRN).
/// </summary>
public readonly struct CompanyRegistrationNumber : IEquatable<CompanyRegistrationNumber>, ISpanParsable<CompanyRegistrationNumber>, ISpanFormattable
{
    private const int CharCount = 8;
    private const int BitsPerChar = 6;

    private readonly UInt48 _value;

    private CompanyRegistrationNumber(UInt48 value)
    {
        _value = value;
    }

    public bool Equals(CompanyRegistrationNumber other)
    {
        return _value.Equals(other._value);
    }

    public override bool Equals(object? obj)
    {
        return obj is CompanyRegistrationNumber other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    public static CompanyRegistrationNumber Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out CompanyRegistrationNumber result))
        {
            return result;
        }

        throw new FormatException("Input string was not in a correct format.");
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out CompanyRegistrationNumber result)
    {
        if (s.Length < CharCount)
        {
            result = default;
            return false;
        }

        UInt48 value = default;

        for (int i = 0; i < CharCount; i++)
        {
            if (CharUtils.TryEncodeAlphanumeric(in s[i], out int encoded))
            {
                value = (value << BitsPerChar) | encoded;
                continue;
            }

            result = default;
            return false;
        }

        result = new CompanyRegistrationNumber(value);
        return true;
    }

    public static CompanyRegistrationNumber Parse(string s, IFormatProvider? provider = null)
    {
        if (TryParse(s, provider, out CompanyRegistrationNumber result))
        {
            return result;
        }

        throw new FormatException("Input string was not in a correct format.");
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out CompanyRegistrationNumber result)
    {
        if (s is null)
        {
            result = default;
            return false;
        }

        return TryParse(s.AsSpan(), provider, out result);
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider = null)
    {
        if (destination.Length < CharCount)
        {
            charsWritten = 0;
            return false;
        }

        for (int i = 0; i < CharCount; i++)
        {
            int shift = (CharCount - 1 - i) * BitsPerChar;
            int index = (int)((_value >> shift) & 0x3F);
            destination[i] = CharUtils.AlphanumericChars[index];
        }

        charsWritten = CharCount;
        return true;
    }

    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        return string.Create(CharCount, this, (span, crn) =>
        {
            crn.TryFormat(span, out _, null, null);
        });
    }

    public override string ToString()
    {
        return ToString(null, null);
    }

    public static bool operator ==(CompanyRegistrationNumber left, CompanyRegistrationNumber right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(CompanyRegistrationNumber left, CompanyRegistrationNumber right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Converts a Company Registration Number to its 64-bit unsigned integer representation.
    /// </summary>
    public static explicit operator ulong(CompanyRegistrationNumber crn)
    {
        return (ulong)crn._value;
    }

    /// <summary>
    /// Converts a 64-bit unsigned integer to its Company Registration Number representation.
    /// </summary>
    public static explicit operator CompanyRegistrationNumber(ulong value)
    {
        return new CompanyRegistrationNumber((UInt48)value);
    }
}
