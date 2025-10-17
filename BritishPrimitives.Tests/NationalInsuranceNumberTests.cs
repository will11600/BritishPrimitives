namespace BritishPrimitives.Tests;

public class NationalInsuranceNumberTests
{
    [Theory]
    [InlineData("AA000000A")]
    [InlineData("AA 00 00 00 A")]
    [InlineData("AB123456C")]
    [InlineData("AB 12 34 56 C")]
    [InlineData("ZY999999D")]
    [InlineData("CR123456B")]
    public void Parse_ValidNINO_ShouldSucceed(string input)
    {
        var result = NationalInsuranceNumber.Parse(input, null);
    }

    [Theory]
    [InlineData("AA000000A")]
    [InlineData("AB123456C")]
    [InlineData("ZY999999D")]
    public void TryParse_ValidNINO_ShouldReturnTrue(string input)
    {
        var success = NationalInsuranceNumber.TryParse(input, null, out var result);
        Assert.True(success);
    }

    [Theory]
    [InlineData("DA000000A", "D in first position")]
    [InlineData("FA000000A", "F in first position")]
    [InlineData("IA000000A", "I in first position")]
    [InlineData("QA000000A", "Q in first position")]
    [InlineData("UA000000A", "U in first position")]
    [InlineData("VA000000A", "V in first position")]
    [InlineData("AD000000A", "D in second position")]
    [InlineData("AF000000A", "F in second position")]
    [InlineData("AI000000A", "I in second position")]
    [InlineData("AQ000000A", "Q in second position")]
    [InlineData("AU000000A", "U in second position")]
    [InlineData("AV000000A", "V in second position")]
    public void Parse_InvalidPrefixForbiddenLetters_ShouldThrow(string input, string reason)
    {
        Assert.Throws<FormatException>(() => NationalInsuranceNumber.Parse(input, null));
    }

    [Theory]
    [InlineData("AO000000A")]
    [InlineData("BO000000A")]
    [InlineData("CO000000A")]
    [InlineData("ZO999999D")]
    public void Parse_InvalidPrefix_OAsSecondLetter_ShouldThrow(string input)
    {
        Assert.Throws<FormatException>(() => NationalInsuranceNumber.Parse(input, null));
    }

    [Theory]
    [InlineData("BG000000A")]
    [InlineData("GB000000A")]
    [InlineData("KN000000A")]
    [InlineData("NK000000A")]
    [InlineData("NT000000A")]
    [InlineData("TN000000A")]
    [InlineData("ZZ000000A")]
    public void Parse_InvalidPrefix_ReservedPrefixes_ShouldThrow(string input)
    {
        Assert.Throws<FormatException>(() => NationalInsuranceNumber.Parse(input, null));
    }

    [Theory]
    [InlineData("AA000000E")]
    [InlineData("AA000000F")]
    [InlineData("AA000000Z")]
    [InlineData("AA0000001")]
    [InlineData("AA000000a")]
    public void Parse_InvalidSuffix_ShouldThrow(string input)
    {
        Assert.Throws<FormatException>(() => NationalInsuranceNumber.Parse(input, null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    [InlineData("AA")]
    [InlineData("AA000000")]
    [InlineData("AA0000000A")]
    [InlineData("AAA000000A")]
    [InlineData("1A000000A")]
    [InlineData("A1000000A")]
    [InlineData("AA00000AA")]
    [InlineData("AAABCDEFG")]
    public void Parse_InvalidFormat_ShouldThrow(string input)
    {
        Assert.Throws<FormatException>(() => NationalInsuranceNumber.Parse(input, null));
    }

    [Theory]
    [InlineData(null)]
    public void Parse_NullInput_ShouldThrow(string? input)
    {
        Assert.Throws<ArgumentNullException>(() => NationalInsuranceNumber.Parse(input, null));
    }

    [Theory]
    [InlineData("AA 00 00 00 A", "AA000000A")]
    [InlineData("AA  00  00  00  A", "AA000000A")]
    [InlineData(" AA000000A ", "AA000000A")]
    [InlineData("AA 000000 A", "AA000000A")]
    public void Parse_WithWhitespace_ShouldNormalizeCorrectly(string input, string expected)
    {
        var nino = NationalInsuranceNumber.Parse(input, null);
        Assert.Equal(expected, nino.ToString());
    }

    [Fact]
    public void Equals_SameValue_ShouldReturnTrue()
    {
        var nino1 = NationalInsuranceNumber.Parse("AA000000A", null);
        var nino2 = NationalInsuranceNumber.Parse("AA000000A", null);

        Assert.True(nino1.Equals(nino2));
        Assert.True(nino1 == nino2);
        Assert.False(nino1 != nino2);
    }

    [Fact]
    public void Equals_DifferentValue_ShouldReturnFalse()
    {
        var nino1 = NationalInsuranceNumber.Parse("AA000000A", null);
        var nino2 = NationalInsuranceNumber.Parse("AB123456C", null);

        Assert.False(nino1.Equals(nino2));
        Assert.False(nino1 == nino2);
        Assert.True(nino1 != nino2);
    }

    [Fact]
    public void Equals_SameValueWithDifferentWhitespace_ShouldReturnTrue()
    {
        var nino1 = NationalInsuranceNumber.Parse("AA000000A", null);
        var nino2 = NationalInsuranceNumber.Parse("AA 00 00 00 A", null);

        Assert.True(nino1.Equals(nino2));
        Assert.True(nino1 == nino2);
    }

    [Fact]
    public void GetHashCode_SameValue_ShouldReturnSameHashCode()
    {
        var nino1 = NationalInsuranceNumber.Parse("AA000000A", null);
        var nino2 = NationalInsuranceNumber.Parse("AA 00 00 00 A", null);

        Assert.Equal(nino1.GetHashCode(), nino2.GetHashCode());
    }

    [Theory]
    [InlineData("AA000000A", "AA 00 00 00 A")]
    [InlineData("AB123456C", "AB 12 34 56 C")]
    public void ToString_ShouldReturnFormattedString(string input, string expectedResult)
    {
        var nino = NationalInsuranceNumber.Parse(input, null);
        Assert.Equal(expectedResult, nino.ToString("s"));
    }

    [Fact]
    public void TryFormat_WithSufficientBuffer_ShouldSucceed()
    {
        var nino = NationalInsuranceNumber.Parse("PY762389A", null);
        Span<char> buffer = stackalloc char[NationalInsuranceNumber.MaxLength];

        var success = nino.TryFormat(buffer, out int charsWritten, default, null);

        Assert.True(success);
        Assert.True(charsWritten > 0);
        Assert.True(charsWritten <= NationalInsuranceNumber.MaxLength);
    }

    [Fact]
    public void TryFormat_WithInsufficientBuffer_ShouldFail()
    {
        var nino = NationalInsuranceNumber.Parse("AA000000A", null);
        Span<char> buffer = stackalloc char[5];

        var success = nino.TryFormat(buffer, out int charsWritten, default, null);

        Assert.False(success);
        Assert.Equal(0, charsWritten);
    }

    [Fact]
    public void SpanParse_ValidNINO_ShouldSucceed()
    {
        ReadOnlySpan<char> input = "AA000000A".AsSpan();
        var result = NationalInsuranceNumber.Parse(input, null);
    }

    [Fact]
    public void TrySpanParse_ValidNINO_ShouldReturnTrue()
    {
        ReadOnlySpan<char> input = "AA000000A".AsSpan();
        var success = NationalInsuranceNumber.TryParse(input, null, out _);
        Assert.True(success);
    }

    [Fact]
    public void TrySpanParse_InvalidNINO_ShouldReturnFalse()
    {
        ReadOnlySpan<char> input = "INVALID".AsSpan();
        var success = NationalInsuranceNumber.TryParse(input, null, out _);
        Assert.False(success);
    }

    [Theory]
    [InlineData("AA000000A")]
    [InlineData("AB123456B")]
    [InlineData("ZY999999C")]
    [InlineData("CR654321D")]
    public void Parse_AllValidSuffixes_ShouldSucceed(string input)
    {
        NationalInsuranceNumber.Parse(input, null);
    }

    [Fact]
    public void TryParse_EmptyString_ShouldReturnFalse()
    {
        var success = NationalInsuranceNumber.TryParse("", null, out _);
        Assert.False(success);
    }

    [Theory]
    [InlineData("AA000000A")]
    [InlineData("AB000000A")]
    [InlineData("AC000000A")]
    [InlineData("AE000000A")]
    [InlineData("AG000000A")]
    [InlineData("AH000000A")]
    [InlineData("AJ000000A")]
    [InlineData("AK000000A")]
    [InlineData("AL000000A")]
    [InlineData("AM000000A")]
    [InlineData("AN000000A")]
    [InlineData("AP000000A")]
    [InlineData("AR000000A")]
    [InlineData("AS000000A")]
    [InlineData("AT000000A")]
    [InlineData("AW000000A")]
    [InlineData("AX000000A")]
    [InlineData("AY000000A")]
    [InlineData("AZ000000A")]
    public void Parse_AllValidSecondLetters_ShouldSucceed(string input)
    {
        var result = NationalInsuranceNumber.Parse(input, null);
    }
}