using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BritishPrimitives;

/// <summary>
/// A compact representation of a UK postcode (postal code) using two 32-bit unsigned integers.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 8)]
public readonly struct BritishPostalCode : IEquatable<BritishPostalCode>, ISpanParsable<BritishPostalCode>, ISpanFormattable
{
    private const string GirobankBootle = "GIR0AA";

    public const int MaxSize = 1 + sizeof(uint) * 2;

    [FieldOffset(0)]
    private readonly uint _outward;

    [FieldOffset(4)]
    private readonly uint _inward;

    private BritishPostalCode(uint outward, uint inward)
    {
        _outward = outward;
        _inward = inward;
    }

    /// <summary>
    /// Returns the outward code (e.g., "M1A", "B33")
    /// </summary>
    public string OutwardCode => Unpack(_outward);

    /// <summary>
    /// Returns the inward code (e.g., "1AA")
    /// </summary>
    public string InwardCode => Unpack(_inward);

    /// <summary>
    /// Converts the postcode to its standard string representation, with the outward and inward codes separated by a space.
    /// </summary>
    /// <returns>A formatted string representation of the postcode (e.g., "SW1A 0AA").</returns>
    public override string ToString()
    {
        return ToString(null, null);
    }

    /// <inheritdoc/>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider = null)
    {
        Span<char> outwardCode = stackalloc char[sizeof(uint)];
        outwardCode = outwardCode[..Unpack(_outward, outwardCode)];

        Span<char> inwardCode = stackalloc char[sizeof(uint)];
        inwardCode = inwardCode[..Unpack(_inward, inwardCode)];

        int maxLength = outwardCode.Length + inwardCode.Length;
        bool includeSpace = !format.ContainsAny('s', 'S');

        if (includeSpace)
        {
            maxLength++;
        }

        if (destination.Length < maxLength)
        {
            charsWritten = 0;
            return false;
        }

        outwardCode.CopyTo(destination);
        charsWritten = outwardCode.Length;

        if (includeSpace)
        {
            destination[charsWritten++] = ' ';
        }

        inwardCode.CopyTo(destination[charsWritten..]);
        charsWritten += inwardCode.Length;

        return true;
    }

    /// <inheritdoc/>
    public string ToString(string? format = null, IFormatProvider? formatProvider = null)
    {
        Span<char> buffer = stackalloc char[sizeof(uint) * 2 + 1];

        if (TryFormat(buffer, out int charsWritten, format, formatProvider))
        {
            return new string(buffer[..charsWritten]);
        }

        throw new FormatException("The format is invalid.");
    }

    /// <summary>
    /// Indicates whether the current object is equal to another <see cref="BritishPostalCode"/>.
    /// </summary>
    /// <param name="other">A <see cref="BritishPostalCode"/> to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <see langword="false"/>.</returns>
    public bool Equals(BritishPostalCode other)
    {
        return _outward == other._outward && _inward == other._inward;
    }

    public override bool Equals(object? obj)
    {
        return obj is BritishPostalCode other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_outward, _inward);
    }

    /// <summary>
    /// Converts the character span representation of a UK postcode to its <see cref="BritishPostalCode"/> equivalent.
    /// </summary>
    /// <param name="s">A span of characters containing the postcode to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information. This parameter is currently ignored.</param>
    /// <returns>A <see cref="BritishPostalCode"/> that is equivalent to the postcode contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException"><paramref name="s"/> is not in a valid UK postcode format.</exception>
    public static BritishPostalCode Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null)
    {
        if (!TryParse(s, provider, out var result))
        {
            throw new FormatException("Invalid UK postcode format.");
        }

        return result;
    }

    /// <summary>
    /// Converts the string representation of a UK postcode to its <see cref="BritishPostalCode"/> equivalent.
    /// </summary>
    /// <param name="s">A string containing the postcode to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information. This parameter is currently ignored.</param>
    /// <returns>A <see cref="BritishPostalCode"/> that is equivalent to the postcode contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException"><paramref name="s"/> is null or not in a valid UK postcode format.</exception>
    public static BritishPostalCode Parse(string s, IFormatProvider? provider = null)
    {
        return Parse(s.AsSpan(), provider);
    }

    /// <summary>
    /// Tries to convert the character span representation of a UK postcode to its <see cref="BritishPostalCode"/> equivalent.
    /// </summary>
    /// <param name="s">A span of characters containing the postcode to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information. This parameter is currently ignored.</param>
    /// <param name="result">When this method returns, contains the <see cref="BritishPostalCode"/> equivalent of <paramref name="s"/> if the conversion succeeded, or a default value if the conversion failed.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method does not throw an exception on failure. It validates against all standard UK postcode patterns and is case-insensitive.
    /// </remarks>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out BritishPostalCode result)
    {
        result = default;

        if (s.IsEmpty)
        {
            return false;
        }

        Span<char> clean = stackalloc char[s.Length];
        clean = clean[..RemoveWhitespace(s, clean)];

        if (clean.Length is < 5 or > 7)
        {
            return false;
        }

        Span<char> inwardCode = clean[^3..];
        Span<char> outwardCode = clean[..^3];

        if (!clean.SequenceEqual(GirobankBootle))
        {
            if (!IsValidInwardCode(inwardCode))
            {
                return false;
            }

            if (!IsValidOutwardCode(outwardCode))
            {
                return false;
            }
        }       

        var outward = Pack(outwardCode, 4);
        var inward = Pack(inwardCode, 4);

        result = new BritishPostalCode(outward, inward);
        return true;
    }

    /// <summary>
    /// Tries to convert the string representation of a UK postcode to its <see cref="BritishPostalCode"/> equivalent.
    /// </summary>
    /// <param name="s">A string containing the postcode to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information. This parameter is currently ignored.</param>
    /// <param name="result">When this method returns, contains the <see cref="BritishPostalCode"/> equivalent of <paramref name="s"/> if the conversion succeeded, or a default value if the conversion failed.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method does not throw an exception on failure. It validates against all standard UK postcode patterns and is case-insensitive.
    /// </remarks>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out BritishPostalCode result)
    {
        return TryParse(s.AsSpan(), provider, out result);
    }

    /// <summary>
    /// Compares two <see cref="BritishPostalCode"/> values for equality.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if the two values are equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(BritishPostalCode left, BritishPostalCode right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Compares two <see cref="BritishPostalCode"/> values for inequality.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns><see langword="true"/> if the two values are not equal; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(BritishPostalCode left, BritishPostalCode right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    /// Explicitly converts a <see cref="BritishPostalCode"/> to its 64-bit unsigned integer representation.
    /// </summary>
    /// <param name="code">The postcode to convert.</param>
    /// <returns>The <see langword="ulong"/> representation of the postcode.</returns>
    public static explicit operator ulong(BritishPostalCode code) => (ulong)code._outward << 32 | code._inward;

    /// <summary>
    /// Explicitly converts a 64-bit unsigned integer to its <see cref="BritishPostalCode"/> representation.
    /// </summary>
    /// <param name="value">The <see langword="ulong"/> value to convert.</param>
    /// <returns>The <see cref="BritishPostalCode"/> representation of the value.</returns>
    public static explicit operator BritishPostalCode(ulong value) => new((uint)(value >> 32), (uint)(value & 0xFFFFFFFF));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int RemoveWhitespace(ReadOnlySpan<char> input, Span<char> output)
    {
        int length = 0;
        for (int i = 0; i < input.Length; i++)
        {
            ref readonly char c = ref input[i];
            if (!char.IsWhiteSpace(c))
            {
                output[length++] = char.ToUpperInvariant(c);
            }
        }
        return length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidInwardCode(ReadOnlySpan<char> inward)
    {
        if (inward.Length != 3 || !char.IsDigit(inward[0]))
        {
            return false;
        }

        for (int i = 1; i < 3; i++)
        {
            ref readonly char c = ref inward[i];
            if (c is < 'A' or > 'Z' || c is 'C' or 'I' or 'K' or 'M' or 'O' or 'V')
            {
                return false;
            }
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidOutwardCode(ReadOnlySpan<char> outward)
    {
        if (outward.Length is < 2 or > 4)
        {
            return false;
        }

        ref readonly char first = ref outward[0];
        if (first is < 'A' or > 'Z' || first is 'I' or 'J' or 'Q' or 'V' or 'X' or 'Z')
        {
            return false;
        }

        return outward.Length switch
        {
            2 => IsValidPattern_A9(outward),
            3 => IsValidPattern_A99_AA9_ANA(outward),
            4 => IsValidPattern_AANN_AANA(outward),
            _ => false
        };
    }

    private static bool IsValidPattern_A9(ReadOnlySpan<char> outward)
    {
        return char.IsLetter(outward[0]) && char.IsDigit(outward[1]);
    }

    private static bool IsValidPattern_A99_AA9_ANA(ReadOnlySpan<char> outward)
    {
        ref readonly char c0 = ref outward[1];
        ref readonly char c1 = ref outward[2];
        return IsValidPattern_A99(in c0, in c1) || IsValidPattern_A99(in c0, in c1) || IsValidPattern_ANA(in c0, in c1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidPattern_A99(ref readonly char c0, ref readonly char c1)
    {
        return char.IsDigit(c0) && char.IsDigit(c1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidPattern_AA9(ref readonly char c0, ref readonly char c1)
    {
        return char.IsLetter(c0)
               && c0 is not 'C' or 'I' or 'K' or 'M' or 'O' or 'V' or 'Q' or 'U' or 'Z'
               && char.IsDigit(c1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidPattern_ANA(ref readonly char c0, ref readonly char c1)
    {
        return char.IsDigit(c0) && char.IsLetter(c1);
    }

    private static bool IsValidPattern_AANN_AANA(ReadOnlySpan<char> outward)
    {
        if (!char.IsLetter(outward[0]) || !char.IsLetter(outward[1]))
        {
            return false;
        }

        ref readonly char c2 = ref outward[2];
        ref readonly char c3 = ref outward[3];

        return IsValidPattern_AANN(in c2, in c3) || IsValidPattern_AANA(in c2, in c3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidPattern_AANN(ref readonly char c2, ref readonly char c3)
    {
        return char.IsDigit(c2) && char.IsDigit(c3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidPattern_AANA(ref readonly char c2, ref readonly char c3)
    {
        return char.IsDigit(c2) && char.IsLetter(c3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Pack(ReadOnlySpan<char> str, int maxLength)
    {
        uint packed = 0;

        int length = Math.Min(str.Length, maxLength);
        for (int i = 0; i < length; i++)
        {
            packed |= (uint)(str[i] & 0xFF) << i * 8;
        }

        return packed;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string Unpack(uint packed)
    {
        Span<char> chars = stackalloc char[sizeof(uint)];
        int count = Unpack(packed, chars);
        return new string(chars[..count]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Unpack(uint packed, Span<char> chars)
    {
        int count = 0;

        for (int i = 0; i < sizeof(uint); i++)
        {
            var c = (char)(packed >> i * 8 & 0xFF);

            if (c == 0)
            {
                break;
            }

            chars[count++] = c;
        }

        return count;
    }
}