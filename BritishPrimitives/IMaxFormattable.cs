namespace BritishPrimitives;

/// <summary>
/// Defines a contract for objects that can be formatted into a span of characters,
/// have a string representation, and have a maximum length constraint for that string
/// representation.
/// </summary>
public interface IMaxFormattable : ISpanFormattable, IMax
{
    // Members are inherited from ISpanFormattable and IMax.
}