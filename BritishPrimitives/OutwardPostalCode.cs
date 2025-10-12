using BritishPrimitives.BitPacking;
using BritishPrimitives.Text;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BritishPrimitives;

/// <summary>
/// Represents the outward part of a United Kingdom postal code, which is the one- to four-character section (e.g., 'SW1A', 'L1')
/// that designates the post town or post area.
/// </summary>
/// Internal Bit Layout (27 bits):
/// ---------------------------------------------------------
/// | Bits 1-6    | Bits 7-13   | Bits 14-20  | Bits 21-27  |
/// |-------------|-------------|-------------|-------------|
/// | Character 1 | Character 2 | Character 3 | Character 4 |
/// | (6 bit)     | (6 bit)     | (6 bit)     | (6 bit)     |
/// ---------------------------------------------------------
[StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
public unsafe struct OutwardPostalCode : IPrimitive<OutwardPostalCode>
{
    internal const int SizeInBytes = 3;

    private const string GirobankBootle = "GIR";

    [FieldOffset(0)]
    private fixed byte _value[SizeInBytes];

    /// <inheritdoc/>
    public static int MaxLength { get; } = 4;

    /// <summary>
    /// The minimum length in characters of the <see langword="string"/> representation of <see cref="OutwardPostalCode"/>.
    /// </summary>
    public static int MinLength { get; } = 2;

    /// <summary>
    /// Converts the span representation of an outward postal code to its <see cref="OutwardPostalCode"/> equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters of the outward postal code to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information. This parameter is ignored.</param>
    /// <returns>An <see cref="OutwardPostalCode"/> equivalent to the value contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException">The format of <paramref name="s"/> is invalid.</exception>
    public static OutwardPostalCode Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out OutwardPostalCode result))
        {
            return result;
        }

        throw new FormatException(Helpers.FormatExceptionMessage);
    }

    /// <summary>
    /// Converts the string representation of an outward postal code to its <see cref="OutwardPostalCode"/> equivalent.
    /// </summary>
    /// <param name="s">A string containing the characters of the outward postal code to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information. This parameter is ignored.</param>
    /// <returns>An <see cref="OutwardPostalCode"/> equivalent to the value contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException">The format of <paramref name="s"/> is invalid.</exception>
    public static OutwardPostalCode Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s.AsSpan(), provider, out OutwardPostalCode result))
        {
            return result;
        }

        throw new FormatException(Helpers.FormatExceptionMessage);
    }

    /// <summary>
    /// Tries to convert the span representation of an outward postal code to its <see cref="OutwardPostalCode"/> equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters of the outward postal code to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information. This parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the <see cref="OutwardPostalCode"/> equivalent to the outward postal code contained in <paramref name="s"/>, if the conversion succeeded, or the default value if the conversion failed.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out OutwardPostalCode result)
    {
        var payload = s.Trim();

        result = new OutwardPostalCode();

        fixed (byte* ptr = result._value)
        {
            BitWriter writer = BitWriter.Create(ptr, SizeInBytes);
            int position = 0;

            int charsPacked = writer.PackCharacters(ref position, payload, Transcoders.AlphanumericLetters);
            if (charsPacked > 0 && charsPacked < payload.Length)
            {
                charsPacked += writer.PackCharacters(ref position, payload[charsPacked..], Transcoders.AlphanumericDigits);
                return (payload.Length - charsPacked) switch
                {
                    0 => true,
                    1 => writer.TryPackCharacter(ref position, payload[charsPacked], Transcoders.AlphanumericLetters),
                    _ => false
                };
            }          
        }

        return payload.Equals(GirobankBootle, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Tries to convert the string representation of an outward postal code to its <see cref="OutwardPostalCode"/> equivalent.
    /// </summary>
    /// <param name="s">A string containing the characters of the outward postal code to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information. This parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the <see cref="OutwardPostalCode"/> equivalent to the outward postal code contained in <paramref name="s"/>, if the conversion succeeded, or the default value if the conversion failed.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out OutwardPostalCode result)
    {
        return TryParse(s.AsSpan(), provider, out result);
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified <see cref="OutwardPostalCode"/> value.
    /// </summary>
    /// <param name="other">An <see cref="OutwardPostalCode"/> value to compare to this instance.</param>
    /// <returns><see langword="true"/> if <paramref name="other"/> has the same value as this instance; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(OutwardPostalCode other)
    {
        return this == other;
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object.
    /// </summary>
    /// <param name="obj">An object to compare to this instance.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> is an <see cref="OutwardPostalCode"/> that has the same value as this instance; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is OutwardPostalCode other && this == other;
    }

    /// <summary>
    /// Converts the value of the current <see cref="OutwardPostalCode"/> object to its equivalent string representation, using the specified format and culture-specific format information.
    /// </summary>
    /// <param name="format">A format string. This parameter is ignored.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information. This parameter is ignored.</param>
    /// <returns>The string representation of the current <see cref="OutwardPostalCode"/>, formatted as specified by the <paramref name="format"/> and <paramref name="formatProvider"/> parameters.</returns>
    /// <exception cref="FormatException">The format of the current <see cref="OutwardPostalCode"/> is invalid for formatting.</exception>
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
    /// Converts the value of the current <see cref="OutwardPostalCode"/> object to its equivalent string representation.
    /// </summary>
    /// <returns>The string representation of the current <see cref="OutwardPostalCode"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        return ToString(null, null);
    }

    /// <summary>
    /// Tries to format the value of the current instance into the provided span of characters.
    /// </summary>
    /// <param name="destination">The span in which to write the characters.</param>
    /// <param name="charsWritten">When this method returns, the number of characters written into <paramref name="destination"/>.</param>
    /// <param name="format">A read-only span containing the format string. This parameter is ignored.</param>
    /// <param name="provider">An optional object that supplies culture-specific formatting information. This parameter is ignored.</param>
    /// <returns><see langword="true"/> if the formatting was successful; otherwise, <see langword="false"/>.</returns>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        int position = 0;

        fixed (byte* ptr = _value)
        {
            BitReader reader = BitReader.Create(ptr, SizeInBytes);
            charsWritten = reader.UnpackCharacters(ref position, destination, Transcoders.Alphanumeric);
        }

        return charsWritten >= MinLength && charsWritten <= MaxLength;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(OutwardPostalCode left, OutwardPostalCode right)
    {
        return Helpers.SequenceEquals(left._value, right._value, SizeInBytes);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(OutwardPostalCode left, OutwardPostalCode right)
    {
        return !Helpers.SequenceEquals(left._value, right._value, SizeInBytes);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator OutwardPostalCode(ulong value)
    {
        OutwardPostalCode result = new();
        Helpers.SpreadBytes(value, result._value, SizeInBytes);
        return result;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator ulong(OutwardPostalCode value)
    {
        return Helpers.ConcatenateBytes(value._value, SizeInBytes);
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
}