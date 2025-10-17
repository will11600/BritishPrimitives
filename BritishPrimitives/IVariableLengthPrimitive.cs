namespace BritishPrimitives;

/// <summary>
/// Defines a contract for primitives whose <see langword="string"/> representation can vary in length.
/// </summary>
/// <typeparam name="TSelf">The type that implements this variable-length primitive contract.</typeparam>
/// <remarks>
/// This extends <see cref="IPrimitive{TSelf}"/> by adding a minimum length constraint.
/// </remarks>
/// <seealso cref="IPrimitive{TSelf}"/>
public interface IVariableLengthPrimitive<TSelf> : IPrimitive<TSelf> where TSelf : struct, IPrimitive<TSelf>
{
    /// <summary>
    /// The minimum length in characters of the <see langword="string"/> representation of <typeparamref name="TSelf"/>.
    /// </summary>
    public static abstract int MinLength { get; }
}
