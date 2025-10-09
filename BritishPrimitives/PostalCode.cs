using BritishPrimitives.BitPacking;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BritishPrimitives;

[StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
public readonly record struct PostalCode : IPrimitive<PostalCode>
{
    private const int SizeInBytes = OutwardPostalCode.SizeInBytes + InwardPostalCode.SizeInBytes;
    private const string GirobankBootle = "GIR0AA";

    private static readonly int _outwardCodeShift = InwardPostalCode.MaxLength * AlphanumericBitPacker.SizeInBits;

    public static int MinLength { get; } = OutwardPostalCode.MaxLength + InwardPostalCode.MaxLength;

    /// <inheritdoc/>
    public static int MaxLength { get; } = MinLength + 1; // +1 for the space

    [FieldOffset(0)]
    public readonly InwardPostalCode inwardCode;

    [FieldOffset(InwardPostalCode.SizeInBytes)]
    public readonly OutwardPostalCode outwardCode;

    public PostalCode(InwardPostalCode inwardCode, OutwardPostalCode outwardCode)
    {
        this.outwardCode = outwardCode;
        this.inwardCode = inwardCode;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PostalCode Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out PostalCode result))
        {
            return result;
        }

        throw new FormatException(Helpers.FormatExceptionMessage);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PostalCode Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s.AsSpan(), provider, out PostalCode result))
        {
            return result;
        }

        throw new FormatException(Helpers.FormatExceptionMessage);
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out PostalCode result)
    {
        Span<Range> ranges = stackalloc Range[3];
        return s.Split(ranges, Character.Whitespace, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) switch
        {
            2 => TryParseInnerAndOutwardCodes(s[ranges[0]], s[ranges[1]], provider, out result),
            1 => TryExtractInwardAndOutwardCodes(s, ranges[0], out var inward, out var outward) && TryParseInnerAndOutwardCodes(inward, outward, provider, out result),
            _ => Helpers.FalseOutDefault(out result)
        };
    }

    private static bool TryExtractInwardAndOutwardCodes(ReadOnlySpan<char> s, Range range, out ReadOnlySpan<char> inwardCode, out ReadOnlySpan<char> outwardCode)
    {
        var payload = s[range];

        if (payload.Length >= InwardPostalCode.MaxLength)
        {
            inwardCode = payload[..InwardPostalCode.MaxLength];
            outwardCode = payload[InwardPostalCode.MaxLength..];

            return true;
        }

        inwardCode = default;
        outwardCode = default;

        return false;
    }

    private static bool TryParseInnerAndOutwardCodes(ReadOnlySpan<char> inwardCodeChars, ReadOnlySpan<char> outwardCodeChars, IFormatProvider? provider, out PostalCode result)
    {
        if (TryParseGirobankBootle(inwardCodeChars, outwardCodeChars, out result))
        {
            return true;
        }

        bool parsedInwardCode = InwardPostalCode.TryParse(inwardCodeChars, provider, out InwardPostalCode inwardCode);
        bool parsedOutwardCode = OutwardPostalCode.TryParse(outwardCodeChars, provider, out OutwardPostalCode outwardCode);
        result = new PostalCode(inwardCode, outwardCode);
        return parsedInwardCode && parsedOutwardCode;
    }

    private static bool TryParseGirobankBootle(ReadOnlySpan<char> inwardCodeChars, ReadOnlySpan<char> outwardCodeChars, out PostalCode result)
    {
        bool inwardMatch = CaseInsensitiveEquals(inwardCodeChars, GirobankBootle, 0, out var girInward);
        bool outwardMatch = CaseInsensitiveEquals(outwardCodeChars, GirobankBootle, inwardCodeChars.Length, out var girOutward);

        if (inwardMatch && outwardMatch)
        {
            InwardPostalCode inwardCode = new(girInward);
            OutwardPostalCode outwardCode = new(girOutward);
            result = new PostalCode(inwardCode, outwardCode);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out PostalCode result)
    {
        return TryParse(s.AsSpan(), provider, out result);
    }

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        return ToString(null, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        return HashCode.Combine(inwardCode, outwardCode);
    }

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

        bool inwardCodeFormatted = inwardCode.TryFormat(destination[InwardPostalCode.MaxLength..], out int inwardCharsWritten, format, provider);
        bool outwardCodeFormatted = outwardCode.TryFormat(destination[^OutwardPostalCode.MaxLength..], out int outwardCharsWritten, format, provider);

        charsWritten += inwardCharsWritten + outwardCharsWritten;
        return inwardCodeFormatted && outwardCodeFormatted && charsWritten == requiredLength;
    }

    public static explicit operator PostalCode(ulong value)
    {
        InwardPostalCode inward = (InwardPostalCode)(value & ((1UL << _outwardCodeShift) - 1));
        OutwardPostalCode outward = (OutwardPostalCode)(value >> _outwardCodeShift);
        return new PostalCode(inward, outward);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator ulong(PostalCode value)
    {
        return (ulong)value.inwardCode | ((ulong)value.outwardCode << _outwardCodeShift);
    }
}
