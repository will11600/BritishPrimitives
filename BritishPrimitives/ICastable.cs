namespace BritishPrimitives;

/// <summary>
/// Defines a contract for a type that can be explicitly converted to and from another value type.
/// </summary>
/// <typeparam name="TSelf">The type that implements this castable contract.</typeparam>
/// <typeparam name="TValue">The underlying value type to cast to and from.</typeparam>
public interface ICastable<TSelf, TValue> where TSelf : ICastable<TSelf, TValue> where TValue : struct
{
    /// <summary>
    /// Defines an explicit conversion from a <typeparamref name="TValue"/> to its <typeparamref name="TSelf"/> representation.
    /// </summary>
    /// <param name="value">The <typeparamref name="TValue"/> to convert.</param>
    /// <returns>The <typeparamref name="TSelf"/> representation of the value.</returns>
    public static abstract explicit operator TSelf(TValue value);

    /// <summary>
    /// Defines an explicit conversion from a <typeparamref name="TSelf"/> to its <typeparamref name="TValue"/> representation.
    /// </summary>
    /// <param name="value">The <typeparamref name="TSelf"/> value to convert.</param>
    /// <returns>The <typeparamref name="TValue"/> representation of the value.</returns>
    public static abstract explicit operator TValue(TSelf value);
}
