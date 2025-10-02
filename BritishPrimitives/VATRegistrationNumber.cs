using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static BritishPrimitives.CharUtils;

namespace BritishPrimitives;

/// <summary>
/// Represents a UK VAT Registration Number, including standard, branch,
/// government, and health authority formats.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public unsafe struct VATRegistrationNumber : IPrimitive<VATRegistrationNumber>
{
    private const string CountryCode = "GB";

    private const int SizeInBytes = 5;

    private const string FormatSpecifiers = "GS";
    private static char GeneralFormatSpecifier => FormatSpecifiers[0];
    private static char SpaceDeliminatedFormatSpecifier => FormatSpecifiers[1];

    /// <inheritdoc/>
    public static int MaxLength { get; } = 12;

    private const int TypeSize = 2;
    private const int FlagSize = 1;
    private const int MainNumberSize = 24;
    private const int BranchCodeSize = 10;
    private const int GovHealthNumberSize = 9;


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

        return payload.Length switch
        {
            5 => TryParseGovHealth(payload, out result),
            9 or 12 => TryParseStandardOrBranch(payload, out result),
            _ => FalseOutDefault(out result)
        };
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

        fixed (byte* ptr = _value)
        {
            var reader = new BitReader(ptr, SizeInBytes);
            var position = 0;
            if (!reader.TryReadBits(ref position, TypeSize, out byte typeAsByte))
            {
                return FalseOutDefault(out charsWritten);
            }

            var type = (VATNumberType)typeAsByte;
            bool space = formatSpecifier == SpaceDeliminatedFormatSpecifier;

            return type switch
            {
                VATNumberType.Standard => TryFormatStandard(destination, in reader, ref position, out charsWritten, space, provider),
                VATNumberType.Branch => TryFormatBranch(destination, in reader, ref position, out charsWritten, space, provider),
                VATNumberType.Government => TryFormatGovernment(destination, in reader, ref position, out charsWritten, provider),
                VATNumberType.Health => TryFormatHealth(destination, in reader, ref position, out charsWritten, provider),
                _ => FalseOutDefault(out charsWritten)
            };
        }
    }

    private static bool TryFormatHealth(Span<char> destination, in BitReader reader, ref int position, out int charsWritten, IFormatProvider? provider)
    {
        if (!reader.TryReadBits(ref position, GovHealthNumberSize, out byte offset))
        {
            return FalseOutDefault(out charsWritten);
        }

        CountryCode.CopyTo(destination);
        charsWritten = CountryCode.Length;

        AppendChars(destination, "HA", ref charsWritten);

        return TryAppendDigitFormat(offset + 500, destination, ref charsWritten, 3, provider);
    }

    private static bool TryFormatGovernment(Span<char> destination, in BitReader reader, ref int position, out int charsWritten, IFormatProvider? provider)
    {
        if (!reader.TryReadBits(ref position, GovHealthNumberSize, out byte number))
        {
            return FalseOutDefault(out charsWritten);
        }

        CountryCode.CopyTo(destination);
        charsWritten = CountryCode.Length;

        AppendChars(destination, "GD", ref charsWritten);

        return TryAppendDigitFormat(number, destination, ref charsWritten, 3, provider);
    }

    private static bool TryFormatBranch(Span<char> destination, in BitReader reader, ref int position, out int charsWritten, bool space, IFormatProvider? provider)
    {
        if (!reader.TryReadBits(ref position, MainNumberSize, out byte mainNumber) || !reader.TryReadBits(ref position, BranchCodeSize, out byte branchCode) || !reader.TryReadBit(ref position, out bool useMod97))
        {
            return FalseOutDefault(out charsWritten);
        }

        int checksum = CalculateChecksum((uint)mainNumber, useMod97);

        CountryCode.CopyTo(destination);
        charsWritten = CountryCode.Length;

        if (space)
        {
            if (!TryAppendMainNumberSpaced(destination, ref charsWritten, provider, mainNumber))
            {
                return false;
            }

            AppendWhitespace(destination, ref charsWritten);

            if (!TryAppendDigitFormat(checksum, destination, ref charsWritten, 2, provider))
            {
                return false;
            }

            AppendWhitespace(destination, ref charsWritten);
        }
        else
        {
            if (!TryAppendDigitFormat(mainNumber, destination, ref charsWritten, 7, provider))
            {
                return false;
            }

            if (!TryAppendDigitFormat(checksum, destination, ref charsWritten, 2, provider))
            {
                return false;
            }
        }

        return TryAppendDigitFormat(branchCode, destination, ref charsWritten, 3, provider);
    }

    private static bool TryFormatStandard(Span<char> destination, in BitReader reader, ref int position, out int charsWritten, bool space, IFormatProvider? provider)
    {
        if (!reader.TryReadBits(ref position, MainNumberSize, out byte mainNumber) || !reader.TryReadBit(ref position, out bool useMod97))
        {
            return FalseOutDefault(out charsWritten);
        }

        int checksum = CalculateChecksum((uint)mainNumber, useMod97);

        CountryCode.CopyTo(destination);
        charsWritten = CountryCode.Length;

        if (space)
        {
            if (!TryAppendMainNumberSpaced(destination, ref charsWritten, provider, mainNumber))
            {
                return FalseOutDefault(out charsWritten);
            }
        }
        else if (!TryAppendDigitFormat(mainNumber, destination, ref charsWritten, 7, provider))
        {
            return FalseOutDefault(out charsWritten);
        }

        return TryAppendDigitFormat(checksum, destination, ref charsWritten, 2, provider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryAppendMainNumberSpaced(Span<char> destination, ref int charsWritten, IFormatProvider? provider, ulong mainNumber)
    {
        Span<char> mainNumberChars = stackalloc char[7];
        if (!TryFormatDigits(mainNumber, mainNumberChars, out int mainNumberCharsWritten, 7, provider))
        {
            return false;
        }
        mainNumberChars = mainNumberChars[..mainNumberCharsWritten];

        AppendChars(destination, mainNumberChars[..3], ref charsWritten);
        AppendWhitespace(destination, ref charsWritten);
        AppendChars(destination, mainNumberChars[4..], ref charsWritten);
        AppendWhitespace(destination, ref charsWritten);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryFormatDigits(ISpanFormattable formattable, Span<char> destination, out int charsWritten, int digitCount, IFormatProvider? provider)
    {
        ReadOnlySpan<char> format = ['D', (char)('0' + digitCount)];
        return formattable.TryFormat(destination, out charsWritten, format, provider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryAppendDigitFormat(ISpanFormattable source, Span<char> destination, ref int charsWritten, int digitCount, IFormatProvider? provider)
    {
        if (TryFormatDigits(source, destination.Slice(charsWritten, digitCount), out int digitCharsWritten, digitCount, provider))
        {
            charsWritten += digitCharsWritten;
            return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendChars(Span<char> destination, ReadOnlySpan<char> source, ref int charsWritten)
    {
        source.CopyTo(destination[..charsWritten]);
        charsWritten += source.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendWhitespace(Span<char> destination, ref int charsWritten)
    {
        destination[charsWritten++] = Whitespace;
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

    private static bool TryParseGovHealth(ReadOnlySpan<char> payload, out VATRegistrationNumber result)
    {
        var code = payload[..2];
        var numberSpan = payload[2..];

        if (!TryParseDigits(numberSpan, out uint number))
        {
            return FalseOutDefault(out result);
        }

        result = new VATRegistrationNumber();
        fixed (byte* ptr = result._value)
        {
            var writer = new BitWriter(ptr, SizeInBytes);
            var position = 0;
            if (code.SequenceEqual("GD"))
            {
                if (number <= 499)
                {
                    return writer.TryWriteBits(ref position, (byte)VATNumberType.Government, TypeSize) && writer.TryWriteBits(ref position, (byte)number, GovHealthNumberSize);
                }
            }
            else if (code.SequenceEqual("HA"))
            {
                if (number >= 500 && number <= 999)
                {
                    return writer.TryWriteBits(ref position, (byte)VATNumberType.Health, TypeSize) && writer.TryWriteBits(ref position, (byte)(number - 500), GovHealthNumberSize);
                }
            }
        }

        return FalseOutDefault(out result);
    }

    private static bool TryParseStandardOrBranch(ReadOnlySpan<char> payload, out VATRegistrationNumber result)
    {
        if (!TryParseDigits(payload[..7], out uint mainNumber) || !TryParseDigits(payload[7..9], out uint providedChecksum))
        {
            return FalseOutDefault(out result);
        }

        int mod97Checksum = CalculateChecksum(mainNumber, useMod97Algorithm: true);
        bool useMod97;

        if (providedChecksum == mod97Checksum)
        {
            useMod97 = true;
        }
        else
        {
            int mod9755Checksum = CalculateChecksum(mainNumber, useMod97Algorithm: false);
            if (providedChecksum == mod9755Checksum)
            {
                useMod97 = false;
            }
            else
            {
                return FalseOutDefault(out result);
            }
        }

        result = new VATRegistrationNumber();
        fixed (byte* ptr = result._value)
        {
            var writer = new BitWriter(ptr, SizeInBytes);
            var position = 0;

            if (payload.Length == 9)
            {
                return writer.TryWriteBits(ref position, (byte)VATNumberType.Standard, TypeSize) &&
                       writer.TryWriteBit(ref position, useMod97) &&
                       writer.TryWriteBits(ref position, (byte)mainNumber, MainNumberSize);
            }

            if (payload.Length == 12)
            {
                if (!TryParseDigits(payload.Slice(9, 3), out uint branchCode))
                {
                    return FalseOutDefault(out result);
                }

                return writer.TryWriteBits(ref position, (byte)VATNumberType.Branch, TypeSize) &&
                       writer.TryWriteBit(ref position, useMod97) &&
                       writer.TryWriteBits(ref position, (byte)mainNumber, MainNumberSize) &&
                       writer.TryWriteBits(ref position, (byte)branchCode, BranchCodeSize);
            }
        }

        return FalseOutDefault(out result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CalculateChecksum(uint sevenDigitNumber, bool useMod97Algorithm)
    {
        int total = 0;
        uint temp = sevenDigitNumber;

        for (int weight = 8; weight >= 2; weight--)
        {
            total += (int)(temp % 10) * weight;
            temp /= 10;
        }

        var remainder = total % 97;

        if (useMod97Algorithm)
        {
            return (97 - remainder);
        }

        var check = remainder > 55 ? 97 - remainder + 55 : 42 - remainder;
        return check > 0 ? check : check + 97;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryParseDigits(ReadOnlySpan<char> s, out uint value)
    {
        value = 0;
        if (s.IsEmpty) return false;

        foreach (char c in s)
        {
            if (c is < '0' or > '9')
            {
                value = 0;
                return false;
            }
            value = (value * 10) + (uint)(c - '0');
        }
        return true;
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