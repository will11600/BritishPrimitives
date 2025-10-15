using BritishPrimitives.BitPacking;
using BritishPrimitives.Text;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BritishPrimitives;

/// <summary>
/// Represents a UK VAT Registration Number, including standard, branch,
/// government, and health authority formats.
/// </summary>
/// Internal Bit Layout (40 bits):
/// ---------------------------------------------
/// | Bits 0-1   | Bits 2-11   | Bits 11-21     |
/// |------------|-------------|----------------|
/// | VAT Type   | Main Number | Branch Code    |
/// | (2 bits)   | (30 bits)   | (10 bits)      |
/// ---------------------------------------------
[StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
public unsafe struct VatRegistrationNumber : IVariableLengthPrimitive<VatRegistrationNumber>, ICastable<VatRegistrationNumber, ulong>
{
    private const string CountryCode = "GB";
    private const string GovernmentDeparmentPrefix = "GD";
    private const string HealthAuthorityPrefix = "HA";

    private const int SizeInBytes = 8;

    private readonly static int _typeSizeInBytes = (int)Enum.GetValues<VatNumberType>().Max();

    /// <inheritdoc/>
    public static int MaxLength { get; } = PrefixLength + SpaceDeliminatedStandardTypeLength + BranchCodeLength + 1;

    /// <inheritdoc/>
    public static int MinLength { get; } = PrefixLength + GovernmentAndHealthAuthorityTypeNumberLength;

    private const int StandardTypeLength = 9;
    private const int GovernmentAndHealthAuthorityTypeNumberLength = 5;
    private const int SpaceDeliminatedStandardTypeLength = StandardTypeLength + 2;
    
    private const int PrefixLength = 2;
    private const int BranchCodeLength = 3;

    private const int MainNumberPrefixDivisor = 100000;
    private const int ReservedPrefixRangeStart = 1;
    private const int ReservedPrefixRangeEnd = 9;
    [FieldOffset(0)]
    private fixed byte _value[SizeInBytes];

    /// <summary>
    /// Converts the string representation of a UK VAT registration number
    /// to its <see cref="VatRegistrationNumber"/> equivalent.
    /// </summary>
    /// <param name="s">A span of characters containing the VAT number to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information (ignored).</param>
    /// <returns>A <see cref="VatRegistrationNumber"/> equivalent to the number contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is not in a valid UK VAT registration number format.
    /// </exception>
    public static VatRegistrationNumber Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out VatRegistrationNumber result))
        {
            return result;
        }

        throw new FormatException(Helpers.FormatExceptionMessage);
    }

    /// <summary>
    /// Converts the string representation of a UK VAT registration number
    /// to its <see cref="VatRegistrationNumber"/> equivalent.
    /// </summary>
    /// <param name="s">A string containing the VAT number to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information (ignored).</param>
    /// <returns>A <see cref="VatRegistrationNumber"/> equivalent to the number contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is <see langword="null"/> or is not in a valid UK VAT registration number format.
    /// </exception>
    public static VatRegistrationNumber Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s.AsSpan(), provider, out VatRegistrationNumber result))
        {
            return result;
        }

        throw new FormatException(Helpers.FormatExceptionMessage);
    }

    /// <summary>
    /// Tries to convert the string representation of a UK VAT registration number
    /// to its <see cref="VatRegistrationNumber"/> equivalent.
    /// </summary>
    /// <param name="s">A span of characters containing the VAT number to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information (ignored).</param>
    /// <param name="result">
    /// When this method returns, contains the <see cref="VatRegistrationNumber"/>
    /// equivalent of the VAT number contained in <paramref name="s"/>, if the conversion
    /// succeeded, or the default value if the conversion failed.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="s"/> was converted successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out VatRegistrationNumber result)
    {
        Span<char> payload = stackalloc char[s.Length];
        payload = payload[..Helpers.StripWhitespace(s, payload)];

        if (payload.Length < MinLength)
        {
            return Helpers.FalseOutDefault(out result);
        }

        result = new VatRegistrationNumber();

        fixed (byte* bytePtr = result._value)
        {
            BitWriter writer = BitWriter.Create(bytePtr, SizeInBytes);
            int position = 0;

            VatNumberType type = ParseType(payload, out int offset);
            return writer.TryPackInteger(ref position, (ulong)type, (ulong)_typeSizeInBytes) && type switch
            {
                VatNumberType.Standard => TryParseStandardType(in writer, ref position, payload[offset..]),
                VatNumberType.Branch => TryParseBranchType(in writer, ref position, payload[offset..]),
                _ => writer.TryPackInteger(ref position, payload[offset..])
            };
        }
    }

    private static bool TryParseStandardType(ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> s)
    {
        return s.Length == StandardTypeLength && TryPackMainNumber(in writer, ref position, s[..StandardTypeLength]);
    }

    private static bool TryParseBranchType(ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> s)
    {
        int length = StandardTypeLength + BranchCodeLength;
        return s.Length == length && TryPackMainNumber(in writer, ref position, s[..StandardTypeLength]) && writer.TryPackInteger(ref position, s[^BranchCodeLength..]);
    }

    private static bool TryPackMainNumber(ref readonly BitWriter writer, ref int position, ReadOnlySpan<char> s)
    {
        bool packed = writer.TryPackInteger(ref position, s, out ulong mainNumber);
        return packed && (mainNumber / MainNumberPrefixDivisor) is < ReservedPrefixRangeStart or > ReservedPrefixRangeEnd;
    }

    private static VatNumberType ParseType(ReadOnlySpan<char> s, out int offset)
    {
        offset = s.StartsWith(CountryCode, StringComparison.OrdinalIgnoreCase) ? CountryCode.Length : 0;
        switch (s.Slice(offset, PrefixLength))
        {
            case var prefix when MemoryExtensions.Equals(prefix, HealthAuthorityPrefix, StringComparison.OrdinalIgnoreCase):
                offset += prefix.Length;
                return VatNumberType.Health;
            case var prefix when MemoryExtensions.Equals(prefix, GovernmentDeparmentPrefix, StringComparison.OrdinalIgnoreCase):
                offset += prefix.Length;
                return VatNumberType.Government;
            default:
                return (s.Length - offset) > StandardTypeLength ? VatNumberType.Branch : VatNumberType.Standard;
        }
    }

    /// <summary>
    /// Tries to convert the specified string representation of a UK VAT registration number
    /// to its <see cref="VatRegistrationNumber"/> equivalent.
    /// </summary>
    /// <param name="s">A string containing the VAT number to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information (ignored).</param>
    /// <param name="result">
    /// When this method returns, contains the <see cref="VatRegistrationNumber"/>
    /// equivalent of the VAT number contained in <paramref name="s"/>, if the conversion
    /// succeeded, or the default value if the conversion failed.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="s"/> was converted successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out VatRegistrationNumber result)
    {
        return TryParse(s.AsSpan(), provider, out result);
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the <paramref name="other"/>
    /// parameter; otherwise, <see langword="false"/>.
    /// </returns>
    public readonly bool Equals(VatRegistrationNumber other)
    {
        return this == other;
    }

    /// <summary>
    /// Tries to format the value of the current VAT registration number into the provided span of characters.
    /// </summary>
    /// <param name="destination">The span to which the VAT registration number is written.</param>
    /// <param name="charsWritten">When this method returns, the number of characters written into <paramref name="destination"/>.</param>
    /// <param name="format">
    /// A read-only span of characters that contains a format specifier indicating how to format the value.
    /// Supported formats: "G" (General/compact) and "S" (Space-delimited).
    /// </param>
    /// <param name="provider">An optional object that supplies culture-specific formatting information (ignored).</param>
    /// <returns>
    /// <see langword="true"/> if the formatting was successful; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The 'G' format (default) produces a compact number, e.g., "GB123456789".
    /// The 'S' format produces a space-delimited number, e.g., "GB 123 4567 89".
    /// </remarks>
    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (!PrimitiveFormat.TryParse(format, out char specifier))
        {
            return Helpers.FalseOutDefault(out charsWritten);
        }

        CountryCode.CopyTo(destination);
        charsWritten = CountryCode.Length;

        if (specifier == PrimitiveFormat.Spaced)
        {
            destination[charsWritten++] = Character.Whitespace;
        }

        fixed (byte* ptr = _value)
        {
            BitReader reader = BitReader.Create(ptr, SizeInBytes);
            int position = 0;

            return reader.TryUnpackInteger(ref position, out ulong typeValue, (ulong)_typeSizeInBytes) && (VatNumberType)typeValue switch
            {
                VatNumberType.Government => TryUnpackLetterPrefixed(GovernmentDeparmentPrefix, in reader, ref position, destination, ref charsWritten),
                VatNumberType.Health => TryUnpackLetterPrefixed(HealthAuthorityPrefix, in reader, ref position, destination, ref charsWritten),
                VatNumberType.Branch => TryUnpackBranch(specifier, in reader, ref position, destination, ref charsWritten),
                _ => TryUnpackStandard(specifier, in reader, ref position, destination, ref charsWritten)
            };
        }
    }

    private static bool TryUnpackLetterPrefixed(string prefix, ref readonly BitReader reader, ref int position, Span<char> destination, ref int charsWritten)
    {
        prefix.CopyTo(destination[charsWritten..]);
        charsWritten += prefix.Length;

        Span<char> slice = destination.Slice(charsWritten, GovernmentAndHealthAuthorityTypeNumberLength);
        int digitsWritten = reader.UnpackInteger(ref position, slice);
        charsWritten += digitsWritten;

        return digitsWritten == slice.Length;
    }

    private static bool TryUnpackStandard(char formatSpecifier, ref readonly BitReader reader, ref int position, Span<char> destination, ref int charsWritten)
    {
        if (formatSpecifier == PrimitiveFormat.Spaced)
        {
            return TryUnpackStandardSpaced(in reader, ref position, destination, charsWritten);
        }

        return TryUnpackStandard(in reader, ref position, destination, charsWritten);
    }

    private static bool TryUnpackStandardSpaced(ref readonly BitReader reader, ref int position, Span<char> destination, int charsWritten)
    {
        if ((destination.Length - charsWritten) < SpaceDeliminatedStandardTypeLength)
        {
            return false;
        }

        Span<char> slice = destination.Slice(charsWritten, SpaceDeliminatedStandardTypeLength);
        int digitsWritten = reader.UnpackInteger(ref position, slice, "000 0000 00");
        return digitsWritten == SpaceDeliminatedStandardTypeLength;
    }

    private static bool TryUnpackStandard(ref readonly BitReader reader, ref int position, Span<char> destination, int charsWritten)
    {
        if ((destination.Length - charsWritten) < StandardTypeLength)
        {
            return false;
        }

        Span<char> slice = destination.Slice(charsWritten, StandardTypeLength);
        int digitsWritten = reader.UnpackInteger(ref position, slice);
        return digitsWritten == StandardTypeLength;
    }

    private static bool TryUnpackBranch(char formatSpecifier, ref readonly BitReader reader, ref int position, Span<char> destination, ref int charsWritten)
    {
        bool standardUnpacked;

        if (formatSpecifier == PrimitiveFormat.Spaced)
        {
            standardUnpacked = TryUnpackStandardSpaced(in reader, ref position, destination, charsWritten);
            destination[charsWritten++] = Character.Whitespace;
        }
        else
        {
            standardUnpacked = TryUnpackStandard(in reader, ref position, destination, charsWritten);
        }

        if ((destination.Length - charsWritten) < BranchCodeLength)
        {
            return false;
        }

        Span<char> slice = destination.Slice(charsWritten, BranchCodeLength);
        int digitsWritten = reader.UnpackInteger(ref position, slice);

        return standardUnpacked && digitsWritten == slice.Length;
    }

    /// <summary>
    /// Converts the value of the current <see cref="VatRegistrationNumber"/> object to its equivalent string representation.
    /// </summary>
    /// <param name="format">
    /// A string that contains the format specifier. Supported formats: "G" (General/compact) and "S" (Space-delimited).
    /// </param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information (ignored).</param>
    /// <returns>The string representation of the current VAT registration number in the specified format.</returns>
    public readonly string ToString(string? format, IFormatProvider? formatProvider)
    {
        Span<char> chars = stackalloc char[14];
        if (TryFormat(chars, out int charsWritten, format is null ? [] : format.AsSpan(), formatProvider))
        {
            return chars[..charsWritten].ToString();
        }

        return string.Empty;
    }

    /// <summary>
    /// Converts the value of the current <see cref="VatRegistrationNumber"/> object to its equivalent string representation
    /// using the general ("G") format.
    /// </summary>
    /// <returns>The string representation of the current VAT registration number.</returns>
    public override readonly string ToString()
    {
        return ToString(null, null);
    }

    /// <inheritdoc/>
    public static explicit operator ulong(VatRegistrationNumber value)
    {
        return Helpers.ConcatenateBytes<ulong>(value._value, SizeInBytes);
    }

    /// <inheritdoc/>
    public static explicit operator VatRegistrationNumber(ulong value)
    {
        VatRegistrationNumber n = new();
        Helpers.SpreadBytes(value, n._value, SizeInBytes);
        return n;
    }

    /// <summary>
    /// Indicates whether this instance and a specified object are equal.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is a <see cref="VatRegistrationNumber"/>
    /// and equals the current instance; otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly bool Equals(object? obj)
    {
        return obj is VatRegistrationNumber number && Equals(number);
    }

    /// <summary>
    /// Returns the hash code for this <see cref="VatRegistrationNumber"/>.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        fixed (byte* ptr = _value)
        {
            return Helpers.BuildHashCode(ptr, SizeInBytes);
        }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(VatRegistrationNumber left, VatRegistrationNumber right)
    {
        return Helpers.SequenceEquals(left._value, right._value, SizeInBytes);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(VatRegistrationNumber left, VatRegistrationNumber right)
    {
        return !Helpers.SequenceEquals(left._value, right._value, SizeInBytes);
    }
}
