namespace BritishPrimitives;

/// <summary>
/// Defines a contract for a primitive type that can be represented by a 64-bit unsigned integer.
/// </summary>
/// <remarks>
/// This interface provides core functionality for equality comparison, parsing from spans, string formatting, and explicit conversions to and from a <see langword="ulong"/>.
/// </remarks>
/// <typeparam name="TSelf">The type that implements this primitive contract.</typeparam>
public interface IPrimitive<TSelf> : IEquatable<TSelf>, ISpanParsable<TSelf>, ISpanFormattable where TSelf : struct, IPrimitive<TSelf>
{
    /// <summary>
    /// Explicitly converts a <typeparamref name="TSelf"/> to its 64-bit unsigned integer representation.
    /// </summary>
    /// <param name="value">The <typeparamref name="TSelf"/> to convert.</param>
    /// <returns>The <see langword="ulong"/> representation of the value.</returns>
    public static abstract explicit operator TSelf(ulong value);

    /// <summary>
    /// Explicitly converts a 64-bit unsigned integer to its <typeparamref name="TSelf"/> representation.
    /// </summary>
    /// <param name="value">The <see langword="ulong"/> value to convert.</param>
    /// <returns>The <typeparamref name="TSelf"/> representation of the value.</returns>
    public static abstract explicit operator ulong(TSelf value);
}
