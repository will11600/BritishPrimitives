using BritishPrimitives.BitPacking;
using BritishPrimitives.Text;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BritishPrimitives;

/// <summary>
/// Represents a UK National Insurance Number (NINo), a unique reference number
/// used in the administration of the UK social security and tax systems.
/// </summary>
/// Internal Bit Layout (34 bits):
/// ----------------------------------------
/// | Bits 0-9  | Bits 10-30  | Bits 31-32 |
/// |-----------|-------------|------------|
/// | Prefix    | Main Number | Suffix     |
/// | (10 bits) | (20 bits)   | (2 bits)   |
/// ----------------------------------------
[StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
public unsafe struct NationalInsuranceNumber : IVariableLengthPrimitive<NationalInsuranceNumber>, ICastable<NationalInsuranceNumber, uint>
{
    private const int SizeInBytes = 4;

    private const string DisallowedPrefixes = "BGGBKNNKNTTNZZ";

    private static readonly SearchValues<char> _disallowedPrefixChars;
    private static readonly CharacterTranscoder _suffixTranscoder;

    /// <inheritdoc/>
    public static int MaxLength { get; }

    /// <inheritdoc/>
    public static int MinLength { get; }

    private const int PrefixLength = 2;
    private const int SuffixLength = 1;
    private const int MainNumberLength = 6;

    [FieldOffset(0)]
    private fixed byte _value[SizeInBytes];

    static NationalInsuranceNumber()
    {
        MaxLength = 13;
        MinLength = 9;

        CharacterRange uppercaseAToD = new(Character.UppercaseA, Character.UppercaseD);
        CharacterRange lowercaseAToD = new(Character.UppercaseA, Character.UppercaseD);
        _suffixTranscoder = new CharacterTranscoder([uppercaseAToD], [uppercaseAToD, lowercaseAToD]);

        _disallowedPrefixChars = SearchValues.Create("DFIQUVdfiquv");
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

        throw new FormatException(Helpers.FormatExceptionMessage);
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
        ArgumentNullException.ThrowIfNull(s, nameof(s));

        if (TryParse(s.AsSpan(), provider, out NationalInsuranceNumber ni))
        {
            return ni;
        }

        throw new FormatException(Helpers.FormatExceptionMessage);
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
        Span<char> payload = stackalloc char[MinLength];
        int charsWritten = Helpers.StripWhitespace(s, payload);

        if (charsWritten != MinLength)
        {
            return Helpers.FalseOutDefault(out result);
        }

        var prefix = payload[..PrefixLength];
        var suffix = payload[^SuffixLength];
        var mainNumber = payload[PrefixLength..^SuffixLength];

        result = new NationalInsuranceNumber();

        fixed (byte* ptr = result._value)
        {
            BitWriter writer = BitWriter.Create(ptr, SizeInBytes);
            int position = 0;
            return TryPackPrefix(in writer, prefix, ref position)
                && writer.TryPackInteger(ref position, mainNumber)
                && writer.TryPackCharacter(ref position, suffix, _suffixTranscoder);
        }
    }

    private static bool TryPackPrefix(ref readonly BitWriter writer, ReadOnlySpan<char> prefix, ref int position)
    {
        for (int i = 0; i < DisallowedPrefixes.Length; i += PrefixLength)
        {
            if (prefix.Equals(DisallowedPrefixes.AsSpan(i, PrefixLength), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        for (int i = 0; i < prefix.Length; i++)
        {
            if (_disallowedPrefixChars.Contains(prefix[i]))
            {
                return false;
            }
        }

        return prefix[^1] is not 'O' or 'o' && writer.PackCharacters(ref position, prefix, Transcoders.Alphabetical) == PrefixLength;
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
            return Helpers.FalseOutDefault(out result);
        }

        return TryParse(s.AsSpan(), provider, out result);
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true"/> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <see langword="false"/>.</returns>
    public readonly bool Equals(NationalInsuranceNumber other)
    {
        return this == other;
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
        Span<char> s = stackalloc char[MaxLength];
        if (TryFormat(s, out int charsWritten, format.AsSpan(), null))
        {
            return s[..charsWritten].ToString();
        }

        throw new FormatException(Helpers.FormatExceptionMessage);
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
        if (!PrimitiveFormat.TryParse(format, out char formatSpecifier))
        {
            return Helpers.FalseOutDefault(out charsWritten);
        }

        Span<char> prefix = stackalloc char[PrefixLength];
        Span<char> mainNumber = stackalloc char[MainNumberLength];
        char suffix;

        fixed (byte* ptr = _value)
        {
            BitReader reader = BitReader.Create(ptr, SizeInBytes);
            int position = 0;

            int prefixCharsUnpacked = reader.UnpackCharacters(ref position, prefix, Transcoders.Alphabetical);
            int mainNumberCharsUnpacked = reader.UnpackInteger(ref position, mainNumber);

            if (prefixCharsUnpacked != prefix.Length 
                || mainNumberCharsUnpacked != mainNumber.Length 
                || !reader.TryUnpackCharacter(ref position, _suffixTranscoder, out suffix))
            {
                return Helpers.FalseOutDefault(out charsWritten);
            }
        }

        if (formatSpecifier == PrimitiveFormat.Spaced)
        {
            return TryFormatSpaced(destination, prefix, mainNumber, suffix, out charsWritten);
        }
        
        return TryFormatCompact(destination, prefix, mainNumber, suffix, out charsWritten);
    }

    private static bool TryFormatCompact(Span<char> destination, Span<char> prefix, Span<char> mainNumber, char suffix, out int charsWritten)
    {
        if (destination.Length < MinLength)
        {
            return Helpers.FalseOutDefault(out charsWritten);
        }

        prefix.CopyTo(destination);
        charsWritten = prefix.Length;

        mainNumber.CopyTo(destination[charsWritten..]);
        charsWritten += mainNumber.Length;

        destination[charsWritten++] = suffix;

        return true;
    }

    private static bool TryFormatSpaced(Span<char> destination, Span<char> prefix, Span<char> mainNumber, char suffix, out int charsWritten)
    {
        if (destination.Length < MaxLength)
        {
            return Helpers.FalseOutDefault(out charsWritten);
        }

        prefix.CopyTo(destination);
        charsWritten = prefix.Length;

        Helpers.Append(destination, Character.Whitespace, ref charsWritten);

        for (int i = 0; i < mainNumber.Length; i += PrefixLength)
        {
            var source = mainNumber.Slice(i, PrefixLength);
            source.CopyTo(destination[charsWritten..]);
            charsWritten += PrefixLength;

            Helpers.Append(destination, Character.Whitespace, ref charsWritten);
        }

        destination[charsWritten++] = suffix;

        return true;
    }

    /// <summary>
    /// Indicates whether this instance and a specified object are equal.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="NationalInsuranceNumber"/> that equals the current instance; otherwise, <see langword="false"/>.</returns>
    public override readonly bool Equals(object? obj)
    {
        return obj is NationalInsuranceNumber ni && Equals(ni);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        fixed (byte* ptr = _value)
        {
            return Helpers.BuildHashCode(ptr, SizeInBytes);
        }
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(NationalInsuranceNumber left, NationalInsuranceNumber right)
    {
        return Helpers.SequenceEquals(left._value, right._value, SizeInBytes);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(NationalInsuranceNumber left, NationalInsuranceNumber right)
    {
        return !Helpers.SequenceEquals(left._value, right._value, SizeInBytes);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator uint(NationalInsuranceNumber ni)
    {
        return Helpers.ConcatenateBytes<uint>(ni._value, SizeInBytes);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator NationalInsuranceNumber(uint value)
    {
        NationalInsuranceNumber ni = new();
        Helpers.SpreadBytes(value, ni._value, SizeInBytes);
        return ni;
    }
}
