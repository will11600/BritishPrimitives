namespace BritishPrimitives;

/// <summary>
/// Defines a contract for value types that represent a primitive concept,
/// providing basic functionality like equality, parsing, and formatting.
/// </summary>
/// <typeparam name="TSelf">The type that implements this primitive contract.</typeparam>
/// <remarks>
/// This interface enforces static abstract members which are checked by the compiler.
/// It combines several foundational interfaces to ensure a consistent primitive representation.
/// </remarks>
public interface IPrimitive<TSelf> : IEquatable<TSelf>, ISpanParsable<TSelf>, ISpanFormattable where TSelf : struct, IPrimitive<TSelf>
{
    /// <summary>
    /// The maximum length in characters of the <see langword="string"/> representation of <typeparamref name="TSelf"/>.
    /// </summary>
    public static abstract int MaxLength { get; }

    /// <summary>
    /// Determines whether two specified <typeparamref name="TSelf"/> objects have the same value.
    /// </summary>
    /// <param name="left">The first <typeparamref name="TSelf"/> to compare.</param>
    /// <param name="right">The second <typeparamref name="TSelf"/> to compare.</param>
    /// <returns><see langword="true"/> if the value of <paramref name="left"/> is the same as the value of <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static abstract bool operator ==(TSelf left, TSelf right);

    /// <summary>
    /// Determines whether two specified <typeparamref name="TSelf"/> objects have different values.
    /// </summary>
    /// <param name="left">The first <typeparamref name="TSelf"/> to compare.</param>
    /// <param name="right">The second <typeparamref name="TSelf"/> to compare.</param>
    /// <returns><see langword="true"/> if the value of <paramref name="left"/> is different from the value of <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
    public static abstract bool operator !=(TSelf left, TSelf right);
}
