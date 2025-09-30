using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static BritishPrimitives.CharUtils;

namespace BritishPrimitives;

[StructLayout(LayoutKind.Explicit)]
public readonly struct VATRegistrationNumber : IPrimitive<VATRegistrationNumber>
{
    private const string CountryCode = "GB";

    private const string FormatSpecifiers = "GS";
    private static char GeneralFormatSpecifier => FormatSpecifiers[0];
    private static char SpaceDeliminatedFormatSpecifier => FormatSpecifiers[1];

    /// <inheritdoc/>
    public static int MaxLength { get; } = 12;

    private const int TypeShift = 38;
    private const ulong TypeMask = 0x3;

    private const int FlagShift = 37;
    private const ulong FlagMask = 0x1;

    private const int MainNumberShift = 13;
    private const ulong MainNumberMask = 0xFFFFFF;
    private const int MainNumberDigits = 7;

    private const int BranchCodeShift = 3;
    private const ulong BranchCodeMask = 0x3FF;
    private const int BranchCodeDigits = 3;

    private const int GovHealthNumberMask = 0x1FF;
    private const int HealthGovDigits = 3;

    private const int ChecksumDigits = 2;

    [FieldOffset(0)]
    private readonly uint _lo;

    [FieldOffset(4)]
    private readonly byte _hi;

    private VATRegistrationNumber(uint lo, byte hi)
    {
        _lo = lo;
        _hi = hi;
    }

    public static VATRegistrationNumber Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out VATRegistrationNumber result))
        {
            return result;
        }

        throw new FormatException("Invalid VAT registration number format.");
    }

    public static VATRegistrationNumber Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s.AsSpan(), provider, out VATRegistrationNumber result))
        {
            return result;
        }

        throw new FormatException("Invalid VAT registration number format.");
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out VATRegistrationNumber result)
    {
        Span<char> sanitized = stackalloc char[s.Length];
        sanitized = sanitized[..TrimAndMakeUpperInvariant(s, sanitized)];

        if (sanitized.Length < MainNumberDigits)
        {
            return FalseOutDefault(out result);
        }

        if (!sanitized.StartsWith(CountryCode))
        {
            return FalseOutDefault(out result);
        }

        var payload = sanitized[CountryCode.Length..];

        if (payload.Length == 5)
        {
            if (TryParseGovHealth(payload, out result))
            {
                return true;
            }
        }

        if (payload.Length == 9 || payload.Length == 12)
        {
            if (TryParseStandardOrBranch(payload, out result))
            {
                return true;
            }
        }

        return FalseOutDefault(out result);
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out VATRegistrationNumber result)
    {
        if (s is null)
        {
            return FalseOutDefault(out result);
        }

        return TryParse(s.AsSpan(), provider, out result);
    }

    public bool Equals(VATRegistrationNumber other)
    {
        return _lo == other._lo && _hi == other._hi;
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        char formatSpecifier = format.IsEmpty ? GeneralFormatSpecifier : char.ToUpperInvariant(format[0]);
        if (format.Length > 1 || !FormatSpecifiers.Contains(formatSpecifier))
        {
            return FalseOutDefault(out charsWritten);
        }

        ulong value = (ulong)this;
        var type = (VATNumberType)((value >> TypeShift) & TypeMask);
        bool space = formatSpecifier == SpaceDeliminatedFormatSpecifier;

        return type switch
        {
            VATNumberType.Standard => TryFormatStandard(destination, value, out charsWritten, space, provider),
            VATNumberType.Branch => TryFormatBranch(destination, value, out charsWritten, space, provider),
            VATNumberType.Government => TryFormatGovernment(destination, value, out charsWritten, provider),
            VATNumberType.Health => TryFormatHealth(destination, value, out charsWritten, provider),
            _ => FalseOutDefault(out charsWritten)
        };
    }

    private static bool TryFormatHealth(Span<char> destination, ulong value, out int charsWritten, IFormatProvider? provider)
    {
        ulong offset = value & GovHealthNumberMask;

        CountryCode.CopyTo(destination);
        charsWritten = CountryCode.Length;

        AppendChars(destination, "HA", ref charsWritten);

        return TryAppendDigitFormat(offset + 500, destination, ref charsWritten, HealthGovDigits, provider);
    }

    private static bool TryFormatGovernment(Span<char> destination, ulong value, out int charsWritten, IFormatProvider? provider)
    {
        ulong number = value & GovHealthNumberMask;

        CountryCode.CopyTo(destination);
        charsWritten = CountryCode.Length;

        AppendChars(destination, "GD", ref charsWritten);

        return TryAppendDigitFormat(number, destination, ref charsWritten, HealthGovDigits, provider);
    }

    private static bool TryFormatBranch(Span<char> destination, ulong value, out int charsWritten, bool space, IFormatProvider? provider)
    {
        ulong mainNumber = (value >> MainNumberShift) & MainNumberMask;
        ulong branchCode = (value >> BranchCodeShift) & BranchCodeMask;
        bool useMod97 = ((value >> FlagShift) & FlagMask) == 0;
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

            if (!TryAppendDigitFormat(checksum, destination, ref charsWritten, ChecksumDigits, provider))
            {
                return false;
            }

            AppendWhitespace(destination, ref charsWritten);
        }
        else
        {
            if (!TryAppendDigitFormat(mainNumber, destination, ref charsWritten, MainNumberDigits, provider))
            {
                return false;
            }

            if (!TryAppendDigitFormat(checksum, destination, ref charsWritten, ChecksumDigits, provider))
            {
                return false;
            }
        }
        
        return TryAppendDigitFormat(branchCode, destination, ref charsWritten, BranchCodeDigits, provider);
    }

    private static bool TryFormatStandard(Span<char> destination, ulong value, out int charsWritten, bool space, IFormatProvider? provider)
    {
        ulong mainNumber = (value >> MainNumberShift) & MainNumberMask;
        bool useMod97 = ((value >> FlagShift) & FlagMask) == 0;
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
        else if (!TryAppendDigitFormat(mainNumber, destination, ref charsWritten, MainNumberDigits, provider))
        {
            return FalseOutDefault(out charsWritten);
        }

        return TryAppendDigitFormat(checksum, destination, ref charsWritten, ChecksumDigits, provider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryAppendMainNumberSpaced(Span<char> destination, ref int charsWritten, IFormatProvider? provider, ulong mainNumber)
    {
        Span<char> mainNumberChars = stackalloc char[MainNumberDigits];
        if (!TryFormatDigits(mainNumber, mainNumberChars, out int mainNumberCharsWritten, MainNumberDigits, provider))
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
        ReadOnlySpan<char> format = ['D', Convert.ToChar(digitCount)];
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

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        Span<char> chars = stackalloc char[14];
        if (TryFormat(chars, out int charsWritten, format is null ? [] : format.AsSpan(), formatProvider))
        {
            return chars[..charsWritten].ToString();
        }

        return string.Empty;
    }

    public override string ToString()
    {
        return ToString(null, null);
    }

    /// <inheritdoc/>
    public static explicit operator ulong(VATRegistrationNumber value)
    {
        return (ulong)value._hi << (sizeof(uint) * BitsPerByte) | value._lo;
    }

    /// <inheritdoc/>
    public static explicit operator VATRegistrationNumber(ulong value)
    {
        return new VATRegistrationNumber((uint)value, (byte)(value >> (sizeof(uint) * BitsPerByte)));
    }

    private static bool TryParseGovHealth(ReadOnlySpan<char> payload, out VATRegistrationNumber result)
    {
        var code = payload[..2];
        var numberSpan = payload[2..];

        if (!TryParseDigits(numberSpan, out uint number))
        {
            return FalseOutDefault(out result);
        }

        if (code.SequenceEqual("GD"))
        {
            if (number <= 499)
            {
                ulong value = ((ulong)VATNumberType.Government << TypeShift) | number;
                result = (VATRegistrationNumber)value;
                return true;
            }
        }
        else if (code.SequenceEqual("HA"))
        {
            if (number >= 500 && number <= 999)
            {
                ulong value = ((ulong)VATNumberType.Health << TypeShift) | (number - 500);
                result = (VATRegistrationNumber)value;
                return true;
            }
        }

        return FalseOutDefault(out result);
    }

    private static bool TryParseStandardOrBranch(ReadOnlySpan<char> payload, out VATRegistrationNumber result)
    {
        if (!TryParseDigits(payload[..7], out uint mainNumber))
        {
            return FalseOutDefault(out result);
        }

        if (!TryParseDigits(payload[7..2], out uint providedChecksum))
        {
            return FalseOutDefault(out result);
        }

        // Check both checksum algorithms
        int mod97Checksum = CalculateChecksum(mainNumber, useMod97Algorithm: true);
        ulong algorithmFlag;

        if (providedChecksum == mod97Checksum)
        {
            algorithmFlag = 0;
        }
        else
        {
            int mod9755Checksum = CalculateChecksum(mainNumber, useMod97Algorithm: false);
            if (providedChecksum == mod9755Checksum)
            {
                algorithmFlag = 1;
            }
            else
            {
                // Invalid checksum
                return FalseOutDefault(out result);
            }
        }

        // Standard 9-digit number
        if (payload.Length == 9)
        {
            ulong value = ((ulong)VATNumberType.Standard << TypeShift)
                        | (algorithmFlag << FlagShift)
                        | (mainNumber << MainNumberShift);
            result = (VATRegistrationNumber)value;
            return true;
        }

        // Branch 12-digit number
        if (payload.Length == 12)
        {
            if (!TryParseDigits(payload.Slice(9, 3), out uint branchCode))
            {
                return FalseOutDefault(out result);
            }

            ulong value = ((ulong)VATNumberType.Branch << TypeShift)
                        | (algorithmFlag << FlagShift)
                        | ((ulong)mainNumber << MainNumberShift)
                        | (branchCode << BranchCodeShift);
            result = (VATRegistrationNumber)value;
            return true;
        }

        return FalseOutDefault(out result);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CalculateChecksum(uint sevenDigitNumber, bool useMod97Algorithm)
    {
        // Standard HMRC modulus-97 algorithm
        // Weights: 8, 7, 6, 5, 4, 3, 2 for the 7 digits

        int total = 0;
        uint temp = sevenDigitNumber;

        for (int weight = 2; weight <= 8; weight++)
        {
            total += (int)(temp % 10) * weight;
            temp /= 10;
        }

        var remainder = total % 97;

        if (useMod97Algorithm)
        {
            return (97 - remainder) % 97;
        }

        // Alternative "97-55" algorithm
        var check = 42 - remainder;
        return check >= 0 ? check : check + 97;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryParseDigits(ReadOnlySpan<char> s, out uint value)
    {
        value = 0;
        if (s.IsEmpty) return false;

        foreach (char c in s)
        {
            if (c is < Zero or > Nine)
            {
                value = 0;
                return false;
            }
            value = (value * 10) + (uint)(c - Zero);
        }
        return true;
    }

    public override bool Equals(object? obj) => obj is VATRegistrationNumber number && Equals(number);
    public override int GetHashCode() => HashCode.Combine(_lo, _hi);

    public static bool operator ==(VATRegistrationNumber left, VATRegistrationNumber right) => left.Equals(right);
    public static bool operator !=(VATRegistrationNumber left, VATRegistrationNumber right) => !left.Equals(right);
}