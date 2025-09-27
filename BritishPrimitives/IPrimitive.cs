namespace BritishPrimitives;

public interface IPrimitive<TSelf> : IEquatable<TSelf>, ISpanParsable<TSelf>, IFormattable where TSelf : struct, IPrimitive<TSelf>
{
    public static abstract explicit operator TSelf(ulong value);

    public static abstract explicit operator ulong(TSelf value);
}
