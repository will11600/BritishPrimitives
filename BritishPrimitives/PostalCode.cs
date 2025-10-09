using BritishPrimitives.BitPacking;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BritishPrimitives;

/// <summary>
/// Represents a complete United Kingdom postal code, consisting of an outward code and an inward code.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
public readonly record struct PostalCode : IPrimitive<PostalCode>
{
    private const int SizeInBytes = OutwardPostalCode.SizeInBytes + InwardPostalCode.SizeInBytes;
    private const string GirobankBootle = "GIR0AA";

    private static readonly int _inwardCodeShift = OutwardPostalCode.MaxLength * AlphanumericBitPacker.SizeInBits;

    /// <summary>
    /// The minimum length in characters of the <see langword="string"/> representation of <see cref="PostalCode"/>.
    /// </summary>
    public static int MinLength { get; } = OutwardPostalCode.MaxLength + InwardPostalCode.MaxLength;

    /// <inheritdoc/>
    public static int MaxLength { get; } = MinLength + 1; // +1 for the space

    /// <summary>
    /// Gets the outward code component of the postal code.
    /// </summary>
    [FieldOffset(0)]
    public readonly OutwardPostalCode outwardCode;

    /// <summary>
    /// Gets the inward code component of the postal code.
    /// </summary>
    [FieldOffset(OutwardPostalCode.SizeInBytes)]
    public readonly InwardPostalCode inwardCode;


    /// <summary>
    /// Initializes a new instance of the <see cref="PostalCode"/> struct with the specified inward and outward codes.
    /// </summary>
    /// <param name="outwardCode">The outward code component.</param>
    /// <param name="inwardCode">The inward code component.</param>
    public PostalCode(OutwardPostalCode outwardCode, InwardPostalCode inwardCode)
    {
        this.outwardCode = outwardCode;
        this.inwardCode = inwardCode;
    }

    /// <summary>
    /// Converts the span representation of a postal code to its <see cref="PostalCode"/> equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters of the postal code to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information. This parameter is ignored.</param>
    /// <returns>A <see cref="PostalCode"/> equivalent to the value contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException">The format of <paramref name="s"/> is invalid.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PostalCode Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out PostalCode result))
        {
            return result;
        }

        throw new FormatException(Helpers.FormatExceptionMessage);
    }

    /// <summary>
    /// Converts the string representation of a postal code to its <see cref="PostalCode"/> equivalent.
    /// </summary>
    /// <param name="s">A string containing the characters of the postal code to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information. This parameter is ignored.</param>
    /// <returns>A <see cref="PostalCode"/> equivalent to the value contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException">The format of <paramref name="s"/> is invalid.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PostalCode Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s.AsSpan(), provider, out PostalCode result))
        {
            return result;
        }

        throw new FormatException(Helpers.FormatExceptionMessage);
    }

    /// <summary>
    /// Tries to convert the span representation of a postal code to its <see cref="PostalCode"/> equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters of the postal code to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information. This parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the <see cref="PostalCode"/> equivalent to the postal code contained in <paramref name="s"/>, if the conversion succeeded, or the default value if the conversion failed.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out PostalCode result)
    {
        Span<Range> ranges = stackalloc Range[3];
        return s.Split(ranges, Character.Whitespace, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) switch
        {
            2 => TryParseInnerAndOutwardCodes(s[ranges[0]], s[ranges[1]], provider, out result),
            1 => TryExtractInwardAndOutwardCodes(s, ranges[0], out var outward, out var inward) && TryParseInnerAndOutwardCodes(outward, inward, provider, out result),
            _ => Helpers.FalseOutDefault(out result)
        };
    }

    private static bool TryExtractInwardAndOutwardCodes(ReadOnlySpan<char> s, Range range, out ReadOnlySpan<char> outwardCode, out ReadOnlySpan<char> inwardCode)
    {
        var payload = s[range];

        if (payload.Length >= MinLength)
        {
            outwardCode = payload[..OutwardPostalCode.MaxLength];
            inwardCode = payload[^InwardPostalCode.MaxLength..];

            return true;
        }

        inwardCode = default;
        outwardCode = default;

        return false;
    }

    private static bool TryParseInnerAndOutwardCodes(ReadOnlySpan<char> outwardCodeChars, ReadOnlySpan<char> inwardCodeChars, IFormatProvider? provider, out PostalCode result)
    {
        if (TryParseGirobankBootle(inwardCodeChars, outwardCodeChars, out result))
        {
            return true;
        }

        bool parsedOutwardCode = OutwardPostalCode.TryParse(outwardCodeChars, provider, out OutwardPostalCode outwardCode);
        bool parsedInwardCode = InwardPostalCode.TryParse(inwardCodeChars, provider, out InwardPostalCode inwardCode);
        result = new PostalCode(outwardCode, inwardCode);
        return parsedOutwardCode && parsedInwardCode;
    }

    private static bool TryParseGirobankBootle(ReadOnlySpan<char> outwardCodeChars, ReadOnlySpan<char> inwardCodeChars, out PostalCode result)
    {
        bool outwardMatch = CaseInsensitiveEquals(outwardCodeChars, GirobankBootle, 0, out var girOutward);
        bool inwardMatch = CaseInsensitiveEquals(inwardCodeChars, GirobankBootle, outwardCodeChars.Length, out var girInward);

        if (outwardMatch && inwardMatch)
        {
            OutwardPostalCode outwardCode = new(girOutward);
            InwardPostalCode inwardCode = new(girInward);
            result = new PostalCode(outwardCode, inwardCode);
            return true;
        }

        return Helpers.FalseOutDefault(out result);
    }

    private static bool CaseInsensitiveEquals(ReadOnlySpan<char> left, string right, int offset, out ReadOnlySpan<char> result)
    {
        if (offset >= 0 && right.Length >= (left.Length + offset))
        {
            result = right.AsSpan(offset, left.Length);
            return left.Equals(result, StringComparison.OrdinalIgnoreCase);
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Tries to convert the string representation of a postal code to its <see cref="PostalCode"/> equivalent.
    /// </summary>
    /// <param name="s">A string containing the characters of the postal code to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information. This parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the <see cref="PostalCode"/> equivalent to the postal code contained in <paramref name="s"/>, if the conversion succeeded, or the default value if the conversion failed.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out PostalCode result)
    {
        return TryParse(s.AsSpan(), provider, out result);
    }

    /// <summary>
    /// Converts the value of the current <see cref="PostalCode"/> object to its equivalent string representation, using the specified format and culture-specific format information.
    /// </summary>
    /// <param name="format">A format string that can include a format specifier for spacing the outward and inward codes (e.g., 'S' for spaced).</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information. This parameter is ignored.</param>
    /// <returns>The string representation of the current <see cref="PostalCode"/>, formatted as specified by the <paramref name="format"/> and <paramref name="formatProvider"/> parameters.</returns>
    /// <exception cref="FormatException">The format of the current <see cref="PostalCode"/> is invalid for formatting.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        Span<char> buffer = stackalloc char[MaxLength];
        if (TryFormat(buffer, out int charsWritten, format.AsSpan(), formatProvider))
        {
            return buffer[..charsWritten].ToString();
        }

        throw new FormatException(Helpers.FormatExceptionMessage);
    }

    /// <summary>
    /// Converts the value of the current <see cref="PostalCode"/> object to its equivalent string representation.
    /// </summary>
    /// <returns>The string representation of the current <see cref="PostalCode"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        return ToString(null, null);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        return HashCode.Combine(inwardCode, outwardCode);
    }

    /// <summary>
    /// Tries to format the value of the current instance into the provided span of characters.
    /// </summary>
    /// <param name="destination">The span in which to write the characters.</param>
    /// <param name="charsWritten">When this method returns, the number of characters written into <paramref name="destination"/>.</param>
    /// <param name="format">A read-only span containing the format string, which can include a format specifier for spacing (e.g., 'S').</param>
    /// <param name="provider">An optional object that supplies culture-specific formatting information. This parameter is ignored.</param>
    /// <returns><see langword="true"/> if the formatting was successful; otherwise, <see langword="false"/>.</returns>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (!PrimitiveFormat.TryParse(format, out char formatSpecifier))
        {
            return Helpers.FalseOutDefault(out charsWritten);
        }

        int requiredLength;
        if (formatSpecifier == PrimitiveFormat.Spaced)
        {
            requiredLength = MaxLength;
            charsWritten = 1; // for the space
        }
        else
        {
            requiredLength = MinLength;
            charsWritten = 0;
        }

        if (destination.Length < requiredLength)
        {
            return Helpers.FalseOutDefault(out charsWritten);
        }

        bool outwardCodeFormatted = outwardCode.TryFormat(destination[OutwardPostalCode.MaxLength..], out int outwardCharsWritten, format, provider);
        bool inwardCodeFormatted = inwardCode.TryFormat(destination[^InwardPostalCode.MaxLength..], out int inwardCharsWritten, format, provider);

        charsWritten += inwardCharsWritten + outwardCharsWritten;
        return inwardCodeFormatted && outwardCodeFormatted && charsWritten == requiredLength;
    }

    /// <inheritdoc/>
    public static explicit operator PostalCode(ulong value)
    {
        OutwardPostalCode outward = (OutwardPostalCode)(value & ((1UL << _inwardCodeShift) - 1));
        InwardPostalCode inward = (InwardPostalCode)(value >> _inwardCodeShift);
        return new PostalCode(outward, inward);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator ulong(PostalCode value)
    {
        return (ulong)value.outwardCode | ((ulong)value.inwardCode << _inwardCodeShift);
    }
}