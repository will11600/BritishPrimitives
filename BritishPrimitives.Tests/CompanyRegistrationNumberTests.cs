using System.Diagnostics.CodeAnalysis;

namespace BritishPrimitives.Tests;

[ExcludeFromCodeCoverage]
public sealed class CompanyRegistrationNumberTests
{
    // A valid CRN with a two-letter prefix.
    private const string ValidLetterPrefixCrn = "SC123456";
    // A valid CRN that is composed of all digits.
    private const string ValidNumericCrn = "01234567";
    // Another valid CRN for inequality checks.
    private const string AnotherValidCrn = "NI654321";

    [Theory]
    [InlineData("SC123456")] // Standard letter prefix
    [InlineData("01234567")]  // All numeric
    [InlineData("oc123456")]  // Lowercase letters
    [InlineData(" 12345678 ")] // Padded with whitespace
    public void Parse_ValidString_ShouldSucceed(string input)
    {
        // Act
        var result = CompanyRegistrationNumber.Parse(input);
        var roundtrippedString = result.ToString();
        var expected = new string([.. input.Where(char.IsLetterOrDigit)]).ToUpperInvariant();

        // Assert
        Assert.Equal(expected, roundtrippedString);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1234567")]   // Too short
    [InlineData("123456789")] // Too long
    [InlineData("ABCDEFGH")]  // All letters (invalid format)
    [InlineData("1234567A")]  // Letter in numeric part
    [InlineData("SC12345$")]  // Invalid character
    public void Parse_InvalidString_ShouldThrowFormatException(string? input)
    {
        // Act
        void act() => CompanyRegistrationNumber.Parse(input!);

        // Assert
        Assert.Throws<FormatException>(act);
    }

    [Fact]
    public void TryParse_ValidString_ShouldReturnTrueAndCorrectValue()
    {
        // Act
        var success = CompanyRegistrationNumber.TryParse(ValidLetterPrefixCrn, null, out var result);

        // Assert
        Assert.True(success);
        Assert.Equal(ValidLetterPrefixCrn, result.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("INVALID")]
    public void TryParse_InvalidString_ShouldReturnFalse(string? input)
    {
        // Act
        var success = CompanyRegistrationNumber.TryParse(input, null, out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TryParse_ValidSpan_ShouldReturnTrueAndCorrectValue()
    {
        // Arrange
        var span = ValidLetterPrefixCrn.AsSpan();

        // Act
        var success = CompanyRegistrationNumber.TryParse(span, null, out var result);

        // Assert
        Assert.True(success);
        Assert.Equal(ValidLetterPrefixCrn, result.ToString());
    }

    [Fact]
    public void TryParse_InvalidSpan_ShouldReturnFalse()
    {
        // Arrange
        var span = "INVALID".AsSpan();

        // Act
        var success = CompanyRegistrationNumber.TryParse(span, null, out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void ToString_ShouldReturnCorrectlyFormattedString()
    {
        // Arrange
        var crn = CompanyRegistrationNumber.Parse(ValidLetterPrefixCrn);

        // Act
        var result = crn.ToString();

        // Assert
        Assert.Equal(ValidLetterPrefixCrn, result.ToString());
    }

    [Fact]
    public void TryFormat_SufficientDestination_ShouldReturnTrueAndWriteValue()
    {
        // Arrange
        var crn = CompanyRegistrationNumber.Parse(ValidNumericCrn);
        Span<char> destination = new char[CompanyRegistrationNumber.MaxLength];

        // Act
        var success = crn.TryFormat(destination, out var charsWritten, [], null);

        // Assert
        Assert.True(success);
        Assert.Equal(charsWritten, CompanyRegistrationNumber.MaxLength);
        Assert.Equal(destination, ValidNumericCrn);
    }

    [Fact]
    public void TryFormat_InsufficientDestination_ShouldReturnFalse()
    {
        // Arrange
        var crn = CompanyRegistrationNumber.Parse(ValidNumericCrn);
        Span<char> destination = new char[CompanyRegistrationNumber.MaxLength - 1];

        // Act
        var success = crn.TryFormat(destination, out var charsWritten, [], null);

        // Assert
        Assert.False(success);
        Assert.Equal(0, charsWritten);
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var crn1 = CompanyRegistrationNumber.Parse(ValidLetterPrefixCrn);
        var crn2 = CompanyRegistrationNumber.Parse(ValidLetterPrefixCrn);

        // Act & Assert
        Assert.True(crn1.Equals(crn2));
        Assert.True(crn1 == crn2);
        Assert.False(crn1 != crn2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var crn1 = CompanyRegistrationNumber.Parse(ValidLetterPrefixCrn);
        var crn2 = CompanyRegistrationNumber.Parse(AnotherValidCrn);

        // Act & Assert
        Assert.False(crn1.Equals(crn2));
        Assert.False(crn1 == crn2);
        Assert.True(crn1 != crn2);
    }

    [Fact]
    public void Equals_WithObject_ShouldReturnCorrectValue()
    {
        // Arrange
        var crn1 = CompanyRegistrationNumber.Parse(ValidLetterPrefixCrn);
        object crn2 = CompanyRegistrationNumber.Parse(ValidLetterPrefixCrn);
        object differentObject = AnotherValidCrn;

        // Act & Assert
        Assert.True(crn1.Equals(crn2));
        Assert.False(crn1.Equals(differentObject));
        Assert.False(crn1.Equals(null));
    }

    [Fact]
    public void GetHashCode_ForEqualObjects_ShouldBeEqual()
    {
        // Arrange
        var crn1 = CompanyRegistrationNumber.Parse(ValidLetterPrefixCrn);
        var crn2 = CompanyRegistrationNumber.Parse(ValidLetterPrefixCrn);

        // Act
        var hashCode1 = crn1.GetHashCode();
        var hashCode2 = crn2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_ForDifferentObjects_ShouldNotBeEqual()
    {
        // Arrange
        var crn1 = CompanyRegistrationNumber.Parse(ValidLetterPrefixCrn);
        var crn2 = CompanyRegistrationNumber.Parse(AnotherValidCrn);

        // Act
        var hashCode1 = crn1.GetHashCode();
        var hashCode2 = crn2.GetHashCode();

        // Assert
        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void ExplicitUlongConversion_ShouldRoundtripCorrectly()
    {
        // Arrange
        var originalCrn = CompanyRegistrationNumber.Parse(ValidLetterPrefixCrn);

        // Act
        var ulongValue = (ulong)originalCrn;
        var roundtrippedCrn = (CompanyRegistrationNumber)ulongValue;

        // Assert
        Assert.Equal(roundtrippedCrn, originalCrn);
        Assert.Equal(roundtrippedCrn.ToString(), originalCrn.ToString());
    }
}