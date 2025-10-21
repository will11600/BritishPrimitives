namespace BritishPrimitives;

/// <summary>
/// Defines a contract for objects that have a minimum length constraint
/// for their <see langword="string"/> representation.
/// </summary>
public interface IMin
{
    /// <summary>
    /// Gets the minimum length in characters of the <see langword="string"/> representation of this object.
    /// </summary>
    /// <value>
    /// An <see langword="int"/> representing the smallest permissible length for the string format of this object.
    /// </value>
    public static abstract int MinLength { get; }
}
