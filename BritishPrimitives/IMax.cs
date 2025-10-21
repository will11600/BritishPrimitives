namespace BritishPrimitives;

/// <summary>
/// Defines a contract for objects that have a maximum length constraint
/// for their <see langword="string"/> representation.
/// </summary>
public interface IMax
{
    /// <summary>
    /// Gets the maximum length in characters of the <see langword="string"/> representation of this object.
    /// </summary>
    /// <value>
    /// An <see langword="int"/> representing the largest permissible length for the string format of this object.
    /// </value>
    public static abstract int MaxLength { get; }
}