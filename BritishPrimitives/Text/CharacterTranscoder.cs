using System.Collections.Immutable;
using System.Numerics;

namespace BritishPrimitives.Text;

internal sealed class CharacterTranscoder
{
    private readonly ImmutableArray<CharacterRange> _inputRanges;
    private readonly ImmutableArray<CharacterRange> _outputRanges;

    public readonly int sizeInBits;

    public CharacterTranscoder(ReadOnlySpan<CharacterRange> outputRanges, ReadOnlySpan<CharacterRange> inputRanges)
    {
        _inputRanges = SortRanges(inputRanges, static (a, b) => a.Start.CompareTo(b.Start));
        _outputRanges = SortRanges(outputRanges, static (a, b) => a.Offset.CompareTo(b.Offset));
        sizeInBits = BitOperations.Log2((uint)_outputRanges.Max(static r => r.Offset + r.Count) - 1U) + 1;
    }

    public bool TryEncode(char value, out int index)
    {
        int low = 0;
        int high = _inputRanges.Length - 1;

        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            var range = _inputRanges[mid];

            if (range.Contains(value))
            {
                index = range.Remap(value);
                return true;
            }

            if (value < range.Start)
            {
                high = mid - 1;
            }
            else
            {
                low = mid + 1;
            }
        }

        return Helpers.FalseOutDefault(out index);
    }

    public bool TryDecode(int index, out char result)
    {
        int low = 0;
        int high = _outputRanges.Length - 1;

        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            var range = _outputRanges[mid];

            if (index >= range.Offset && index < (range.Offset + range.Count))
            {
                result = range.Normalize(index);
                return true;
            }

            if (index < range.Offset)
            {
                high = mid - 1;
            }
            else
            {
                low = mid + 1;
            }
        }

        return Helpers.FalseOutDefault(out result);
    }

    private static ImmutableArray<CharacterRange> SortRanges(ReadOnlySpan<CharacterRange> ranges, Comparison<CharacterRange> comparison)
    {
        var builder = ImmutableArray.CreateBuilder<CharacterRange>(ranges.Length);
        builder.AddRange(ranges);
        builder.Sort(comparison);
        return builder.ToImmutable();
    }
}
