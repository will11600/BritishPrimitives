using System.Collections;
using System.Runtime.CompilerServices;

namespace BritishPrimitives.Text;

internal readonly record struct CharacterRange : IReadOnlyCollection<char>
{
    public char Start { get; init; }

    public char End { get; init; }

    public int Offset { get; init; }

    public int Count { get; }

    public CharacterRange(char start, char end, int offset = 0)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(end, start, nameof(end));
        Start = start;
        End = end;
        Offset = offset;
        Count = End - Start + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(char value)
    {
        return value >= Start && value <= End;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Remap(char value)
    {
        return value - Start + Offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public char Normalize(int index)
    {
        return (char)(index + Start - Offset);
    }

    public IEnumerator<char> GetEnumerator()
    {
        for (int i = Start; i <= End; i++)
        {
            yield return (char)i;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}