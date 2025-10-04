using BritishPrimitives.BitPacking;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static BritishPrimitives.CharUtils;

namespace BritishPrimitives;

/// <summary>
/// Represents a UK VAT Registration Number, including standard, branch,
/// government, and health authority formats.
/// </summary>
/// Internal Bit Layout (40 bits):
/// ------------------------------------------------------------------
/// | Bits 0-1   | Bit 2         | Bits 3-13      | Bits 14-38       |
/// |------------|---------------|----------------|------------------|
/// | VAT Type   | UseMod97 Flag | Branch Code    | Main Number      |
/// | (2 bits)   | (1 bit)       | (10 bits)      | (24 bits)        |
/// ------------------------------------------------------------------
[StructLayout(LayoutKind.Explicit)]
public unsafe struct VATRegistrationNumber : IPrimitive<VATRegistrationNumber>
{
    private const ushort Spaces = 0b000100100001000100;

    private const string CountryCode = "GB";

    private const int SizeInBytes = 5;

    private const string FormatSpecifiers = "GS";
    private static char GeneralFormatSpecifier => FormatSpecifiers[0];
    private static char SpaceDeliminatedFormatSpecifier => FormatSpecifiers[1];

    /// <inheritdoc/>
    public static int MaxLength { get; } = 14;
    private const int MinLength = 7;

    private const int PrefixLength = 2;

    private const int MainNumberSize = 24;
    private const int MainNumberLength = 7;

    private const int BranchCodeSize = 10;
    private const int BranchCodeLength = 3;

    private const int GovernmentAndHealthAuthorityTypeNumberLength = 5;

    private const int BranchTypeNumberLength = 12;

    private const int StandardTypeLength = 9;

    private const int UseMod97FlagLength = 1;

    private const int ChecksumLength = 2;

    private const string GovernmentDeparmentPrefix = "GD";
    private const string HealthAuthorityPrefix = "HA";
    private const int MainNumberPrefixDivisor = 100000;
    private const int ReservedPrefixRangeStart = 1;
    private const int ReservedPrefixRangeEnd = 9;
    [FieldOffset(0)]
    private fixed byte _value[SizeInBytes];

    /// <summary>
    /// Converts the string representation of a UK VAT registration number
    /// to its <see cref="VATRegistrationNumber"/> equivalent.
    /// </summary>
    /// <param name="s">A span of characters containing the VAT number to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information (ignored).</param>
    /// <returns>A <see cref="VATRegistrationNumber"/> equivalent to the number contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is not in a valid UK VAT registration number format.
    /// </exception>
    public static VATRegistrationNumber Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out VATRegistrationNumber result))
        {
            return result;
        }

        throw new FormatException("Invalid VAT registration number format.");
    }

    /// <summary>
    /// Converts the string representation of a UK VAT registration number
    /// to its <see cref="VATRegistrationNumber"/> equivalent.
    /// </summary>
    /// <param name="s">A string containing the VAT number to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information (ignored).</param>
    /// <returns>A <see cref="VATRegistrationNumber"/> equivalent to the number contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException">
    /// <paramref name="s"/> is <see langword="null"/> or is not in a valid UK VAT registration number format.
    /// </exception>
    public static VATRegistrationNumber Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s.AsSpan(), provider, out VATRegistrationNumber result))
        {
            return result;
        }

        throw new FormatException("Invalid VAT registration number format.");
    }

    /// <summary>
    /// Tries to convert the string representation of a UK VAT registration number
    /// to its <see cref="VATRegistrationNumber"/> equivalent.
    /// </summary>
    /// <param name="s">A span of characters containing the VAT number to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information (ignored).</param>
    /// <param name="result">
    /// When this method returns, contains the <see cref="VATRegistrationNumber"/>
    /// equivalent of the VAT number contained in <paramref name="s"/>, if the conversion
    /// succeeded, or the default value if the conversion failed.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="s"/> was converted successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out VATRegistrationNumber result)
    {
        Span<char> sanitized = stackalloc char[s.Length];
        if (!TryParseAlphanumericUpperInvariant(s, sanitized, MinLength, MaxLength, out int charsWritten) || !sanitized.StartsWith(CountryCode))
        {
            return FalseOutDefault(out result);
        }

        var payload = sanitized[CountryCode.Length..charsWritten];
        VATRegistrationNumber vatNumber = new();
        BitWriter writer = new(vatNumber._value, SizeInBytes);
        int position = PrefixLength;
        VATNumberType type;

        switch (payload.Length)
        {
            case GovernmentAndHealthAuthorityTypeNumberLength:
                switch (payload[..PrefixLength])
                {
                    case GovernmentDeparmentPrefix:
                        type = VATNumberType.Government;
                        goto WriteDigitsVerbatim;
                    case HealthAuthorityPrefix:
                        type = VATNumberType.Health;
                        goto WriteDigitsVerbatim;
                }
                goto default;
            case StandardTypeLength:
                type = VATNumberType.Standard;
                position += BranchCodeSize;
                break;
            case BranchTypeNumberLength:
                type = VATNumberType.Branch;
                if (writer.TryWriteNumber(ref position, payload[^3..], BranchCodeSize))
                {
                    break;
                }
                goto default;
            default:
                return false;
        }

        if (!uint.TryParse(payload[..7], out uint mainNumber) || HasReservedPrefix(mainNumber) || !int.TryParse(payload[7..9], out int providedChecksum))
        {
            return false;
        }

        Checksum checksum = new(mainNumber);
        for (int i = 0; i < 2; i++)
        {
            bool useMod97 = i == 1;

            if ((useMod97 ? checksum.Mod97 : checksum.Standard) != providedChecksum)
            {
                useMod97 = true;
                continue;
            }

            if (!writer.TryWriteBit(ref position, useMod97) || !writer.TryWriteNumber(ref position, mainNumber, MainNumberSize))
            {
                return false;
            }

            goto WriteType;
        }

        return false;

    WriteDigitsVerbatim:
        position += BranchTypeNumberLength + UseMod97FlagLength;
        var numberPart = payload[(PrefixLength + UseMod97FlagLength)..];
        if (!writer.TryWriteNumber(ref position, numberPart, numberPart.Length))
        {
            return false;
        }
    WriteType:
        position = 0;
        return writer.TryWriteBits(ref position, (byte)type, PrefixLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool HasReservedPrefix(uint mainNumber)
    {
        return mainNumber / MainNumberPrefixDivisor is > ReservedPrefixRangeStart and < ReservedPrefixRangeEnd;
    }

    /// <summary>
    /// Tries to convert the specified string representation of a UK VAT registration number
    /// to its <see cref="VATRegistrationNumber"/> equivalent.
    /// </summary>
    /// <param name="s">A string containing the VAT number to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information (ignored).</param>
    /// <param name="result">
    /// When this method returns, contains the <see cref="VATRegistrationNumber"/>
    /// equivalent of the VAT number contained in <paramref name="s"/>, if the conversion
    /// succeeded, or the default value if the conversion failed.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="s"/> was converted successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out VATRegistrationNumber result)
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
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the <paramref name="other"/>
    /// parameter; otherwise, <see langword="false"/>.
    /// </returns>
    public readonly bool Equals(VATRegistrationNumber other)
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
        char formatSpecifier = format.IsEmpty ? GeneralFormatSpecifier : char.ToUpperInvariant(format[0]);
        if (format.Length > 1 || !FormatSpecifiers.Contains(formatSpecifier))
        {
            return FalseOutDefault(out charsWritten);
        }

        charsWritten = 0;
        Append(destination, CountryCode, ref charsWritten);

        Bit useMod97 = Bit.Error;

        fixed (byte* bytePtr = _value)
        {
            BitReader reader = new(bytePtr, SizeInBytes);

            int position = 0;

            if (!reader.TryReadBits(ref position, PrefixLength, out byte prefixByte))
            {
                return false;
            }

            switch ((VATNumberType)prefixByte)
            {
                case VATNumberType.Government:
                    Append(destination, GovernmentDeparmentPrefix, ref charsWritten);
                    goto WriteGovernmentOrHealthAuthorityNumber;
                case VATNumberType.Health:
                    Append(destination, HealthAuthorityPrefix, ref charsWritten);
                    goto WriteGovernmentOrHealthAuthorityNumber;
                case VATNumberType.Standard:
                    useMod97 = reader.ReadBit(ref position);
                    position += BranchCodeSize;
                    goto WriteMainNumberOnly;
                case VATNumberType.Branch:
                    useMod97 = reader.ReadBit(ref position);
                    goto WriteMainNumberAndBranchCode;
            }

        WriteMainNumberOnly:
            if (TryWriteMainNumber(in reader, ref position, destination, ref charsWritten, useMod97))
            {
                goto InsertSpaces;
            }
            return false;

        WriteMainNumberAndBranchCode:
            if (reader.TryRead(ref position, BranchCodeSize, out ushort branchCode)
                && TryWriteMainNumber(in reader, ref position, destination, ref charsWritten, useMod97)
                && TryFormatDigits(branchCode, destination, BranchCodeLength, ref charsWritten))
            {
                goto InsertSpaces;
            }
            return false;

        WriteGovernmentOrHealthAuthorityNumber:
            if (WriteGovernmentOrHealthAuthorityNumber(in reader, ref position, destination, ref charsWritten))
            {
                goto InsertSpaces;
            }
            return false;
        }

    InsertSpaces:
        if (formatSpecifier != SpaceDeliminatedFormatSpecifier)
        {
            return true;
        }

        for (int i = 0; i < charsWritten; i++)
        {
            if (!ShouldInsertSpace(i))
            {
                continue;
            }

            var current = destination[i..charsWritten];
            var moved = destination.Slice(i + 1, charsWritten - i);
            current.CopyTo(moved);
            destination[i] = Whitespace;
            charsWritten++;
        }

        return true;
    }

    private static bool TryWriteMainNumber(ref readonly BitReader reader, ref int position, Span<char> destination, ref int charsWritten, Bit useMod97)
    {
        if (!reader.TryRead(ref position, MainNumberSize, out uint mainNumber) || !TryFormatDigits(mainNumber, destination, MainNumberLength, ref charsWritten))
        {
            return false;
        }
        Checksum checksum = new(mainNumber);
        return useMod97 switch
        {
            Bit.False => TryFormatDigits(checksum.Standard, destination, ChecksumLength, ref charsWritten),
            Bit.True => TryFormatDigits(checksum.Mod97, destination, ChecksumLength, ref charsWritten),
            _ => false
        };
    }

    private static bool WriteGovernmentOrHealthAuthorityNumber(ref readonly BitReader reader, ref int position, Span<char> destination, ref int charsWritten)
    {
        position += BranchCodeSize + UseMod97FlagLength;
        return reader.TryRead(ref position, GovernmentAndHealthAuthorityTypeNumberLength, out uint result)
               && TryFormatDigits(result, destination, GovernmentAndHealthAuthorityTypeNumberLength, ref charsWritten);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ShouldInsertSpace(int index)
    {
        return ((Spaces >> index) & 1) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Append(Span<char> destination, ReadOnlySpan<char> chars, ref int charsWritten)
    {
        chars.CopyTo(destination[charsWritten..]);
        charsWritten += chars.Length;
    }

    /// <summary>
    /// Converts the value of the current <see cref="VATRegistrationNumber"/> object to its equivalent string representation.
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
    /// Converts the value of the current <see cref="VATRegistrationNumber"/> object to its equivalent string representation
    /// using the general ("G") format.
    /// </summary>
    /// <returns>The string representation of the current VAT registration number.</returns>
    public override readonly string ToString()
    {
        return ToString(null, null);
    }

    /// <inheritdoc/>
    public static explicit operator ulong(VATRegistrationNumber value)
    {
        return FixedSizeBufferExtensions.ConcatenateBytes(value._value, SizeInBytes);
    }

    /// <inheritdoc/>
    public static explicit operator VATRegistrationNumber(ulong value)
    {
        VATRegistrationNumber n = new();
        FixedSizeBufferExtensions.SpreadBytes(value, n._value, SizeInBytes);
        return n;
    }

    /// <summary>
    /// Indicates whether this instance and a specified object are equal.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> is a <see cref="VATRegistrationNumber"/>
    /// and equals the current instance; otherwise, <see langword="false"/>.
    /// </returns>
    public override readonly bool Equals(object? obj)
    {
        return obj is VATRegistrationNumber number && Equals(number);
    }

    /// <summary>
    /// Returns the hash code for this <see cref="VATRegistrationNumber"/>.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        fixed (byte* ptr = _value)
        {
            return FixedSizeBufferExtensions.BuildHashCode(ptr, SizeInBytes);
        }
    }

    /// <summary>
    /// Determines whether two specified <see cref="VATRegistrationNumber"/> objects have the same value.
    /// </summary>
    /// <param name="left">The first <see cref="VATRegistrationNumber"/> to compare.</param>
    /// <param name="right">The second <see cref="VATRegistrationNumber"/> to compare.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> are equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(VATRegistrationNumber left, VATRegistrationNumber right)
    {
        return FixedSizeBufferExtensions.SequenceEquals(left._value, right._value, SizeInBytes);
    }

    /// <summary>
    /// Determines whether two specified <see cref="VATRegistrationNumber"/> objects have different values.
    /// </summary>
    /// <param name="left">The first <see cref="VATRegistrationNumber"/> to compare.</param>
    /// <param name="right">The second <see cref="VATRegistrationNumber"/> to compare.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> are not equal;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(VATRegistrationNumber left, VATRegistrationNumber right)
    {
        return !(left == right);
    }
}
