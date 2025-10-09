using BritishPrimitives.BitPacking;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BritishPrimitives;

/// <summary>
/// Represents the inward part of a United Kingdom postal code, which is the three-character section (e.g., '3AA')
/// that designates the post office delivery area.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = SizeInBytes)]
public unsafe struct InwardPostalCode : IPrimitive<InwardPostalCode>
{
    internal const int SizeInBytes = 3;

    private static readonly SearchValues<char> _invalidLetters = SearchValues.Create("CIKMOVcikmov");

    [FieldOffset(0)]
    private fixed byte _value[SizeInBytes];

    /// <inheritdoc/>
    public static int MaxLength { get; } = 3;

    internal InwardPostalCode(ReadOnlySpan<char> chars)
    {
        int position = 0;

        fixed (byte* ptr = _value)
        {
            BitWriter writer = BitWriter.Create(ptr, SizeInBytes);
            writer.PackLettersAndNumbers(ref position, chars);
        }
    }

    /// <summary>
    /// Converts the span representation of an inward postal code to its <see cref="InwardPostalCode"/> equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters of the inward postal code to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information. This parameter is ignored.</param>
    /// <returns>An <see cref="InwardPostalCode"/> equivalent to the value contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException">The format of <paramref name="s"/> is invalid.</exception>
    public static InwardPostalCode Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out InwardPostalCode result))
        {
            return result;
        }

        throw new FormatException(Helpers.FormatExceptionMessage);
    }

    /// <summary>
    /// Converts the string representation of an inward postal code to its <see cref="InwardPostalCode"/> equivalent.
    /// </summary>
    /// <param name="s">A string containing the characters of the inward postal code to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information. This parameter is ignored.</param>
    /// <returns>An <see cref="InwardPostalCode"/> equivalent to the value contained in <paramref name="s"/>.</returns>
    /// <exception cref="FormatException">The format of <paramref name="s"/> is invalid.</exception>
    public static InwardPostalCode Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s.AsSpan(), provider, out InwardPostalCode result))
        {
            return result;
        }

        throw new FormatException(Helpers.FormatExceptionMessage);
    }

    /// <summary>
    /// Tries to convert the span representation of an inward postal code to its <see cref="InwardPostalCode"/> equivalent.
    /// </summary>
    /// <param name="s">A span containing the characters of the inward postal code to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information. This parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the <see cref="InwardPostalCode"/> equivalent to the inward postal code contained in <paramref name="s"/>, if the conversion succeeded, or the default value if the conversion failed.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out InwardPostalCode result)
    {
        int offset = Character.SkipWhitespace(s);
        int length;

        var payload = s[offset..];

        result = new InwardPostalCode();
        fixed (byte* ptr = result._value)
        {
            BitWriter writer = BitWriter.Create(ptr, SizeInBytes);
            int position = 0;

            if (!writer.TryPackAlphanumericLetter(ref position, payload[0]))
            {
                return Helpers.FalseOutDefault(out result);
            }

            for (length = 1; length < payload.Length; length++)
            {
                ref readonly char c = ref payload[length];
                if (!_invalidLetters.Contains(c) && writer.TryPackAlphanumericLetter(ref position, c))
                {
                    continue;
                }

                return false;
            }
        }

        return length == MaxLength;
    }

    /// <summary>
    /// Tries to convert the string representation of an inward postal code to its <see cref="InwardPostalCode"/> equivalent.
    /// </summary>
    /// <param name="s">A string containing the characters of the inward postal code to convert.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information. This parameter is ignored.</param>
    /// <param name="result">When this method returns, contains the <see cref="InwardPostalCode"/> equivalent to the inward postal code contained in <paramref name="s"/>, if the conversion succeeded, or the default value if the conversion failed.</param>
    /// <returns><see langword="true"/> if <paramref name="s"/> was converted successfully; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out InwardPostalCode result)
    {
        return TryParse(s.AsSpan(), provider, out result);
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified <see cref="InwardPostalCode"/> value.
    /// </summary>
    /// <param name="other">An <see cref="InwardPostalCode"/> value to compare to this instance.</param>
    /// <returns><see langword="true"/> if <paramref name="other"/> has the same value as this instance; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(InwardPostalCode other)
    {
        return this == other;
    }

    /// <summary>
    /// Returns a value indicating whether this instance is equal to a specified object.
    /// </summary>
    /// <param name="obj">An object to compare to this instance.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> is an <see cref="InwardPostalCode"/> that has the same value as this instance; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override readonly bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is InwardPostalCode other && this == other;
    }

    /// <summary>
    /// Converts the value of the current <see cref="InwardPostalCode"/> object to its equivalent string representation, using the specified format and culture-specific format information.
    /// </summary>
    /// <param name="format">A format string. This parameter is ignored.</param>
    /// <param name="formatProvider">An object that supplies culture-specific formatting information. This parameter is ignored.</param>
    /// <returns>The string representation of the current <see cref="InwardPostalCode"/>, formatted as specified by the <paramref name="format"/> and <paramref name="formatProvider"/> parameters.</returns>
    /// <exception cref="FormatException">The format of the current <see cref="InwardPostalCode"/> is invalid for formatting.</exception>
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
    /// Converts the value of the current <see cref="InwardPostalCode"/> object to its equivalent string representation.
    /// </summary>
    /// <returns>The string representation of the current <see cref="InwardPostalCode"/>.</returns>
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

        fixed (byte* pValue = _value)
        {
            BitReader reader = BitReader.Create(pValue, SizeInBytes);
            charsWritten = reader.UnpackLettersAndNumbers(ref position, destination);
        }

        return charsWritten == MaxLength;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(InwardPostalCode left, InwardPostalCode right)
    {
        return Helpers.SequenceEquals(left._value, right._value, SizeInBytes);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(InwardPostalCode left, InwardPostalCode right)
    {
        return !Helpers.SequenceEquals(left._value, right._value, SizeInBytes);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator InwardPostalCode(ulong value)
    {
        InwardPostalCode result = new();
        Helpers.SpreadBytes(value, result._value, SizeInBytes);
        return result;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator ulong(InwardPostalCode value)
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