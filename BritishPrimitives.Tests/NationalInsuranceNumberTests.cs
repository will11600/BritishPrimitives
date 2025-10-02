using System.Diagnostics.CodeAnalysis;

namespace BritishPrimitives.Tests;

[ExcludeFromCodeCoverage]
public sealed class NationalInsuranceNumberTests
{
    // =================================================================================
    // Parsing Tests
    // =================================================================================

    [Theory]
    [InlineData("AB123456C", "AB123456C")]
    [InlineData("ab123456c", "AB123456C")]
    [InlineData("CE191295D", "CE191295D")]
    [InlineData("ZY987654A", "ZY987654A")]
    [InlineData("QQ 12 34 56 C", "QQ123456C")]
    [InlineData(" ab 123456 a ", "AB123456A")]
    [InlineData("  ce191295d  ", "CE191295D")]
    public void TryParse_ShouldSucceed_WithValidFormats(string input, string expectedToString)
    {
        // Act
        bool success = NationalInsuranceNumber.TryParse(input, null, out var nino);

        // Assert
        Assert.True(success);
        Assert.Equal(expectedToString, nino.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("AB12345C")]         // Too short
    [InlineData("AB1234567C")]       // Too long
    [InlineData("AB12345!C")]        // Invalid character
    [InlineData("1B123456C")]        // Digit in prefix
    [InlineData("A1123456C")]        // Digit in prefix
    [InlineData("AB12C456D")]        // Letter in digits
    [InlineData("AB1234567")]        // No suffix letter
    [InlineData("DF123456A")]        // Invalid first prefix char 'D'
    [InlineData("FI123456B")]        // Invalid first prefix char 'F'
    [InlineData("IG123456C")]        // Invalid first prefix char 'I'
    [InlineData("QW123456D")]        // Invalid first prefix char 'Q'
    [InlineData("UC123456A")]        // Invalid first prefix char 'U'
    [InlineData("VB123456B")]        // Invalid first prefix char 'V'
    [InlineData("AD123456A")]        // Invalid second prefix char 'D'
    [InlineData("AF123456B")]        // Invalid second prefix char 'F'
    [InlineData("AI123456C")]        // Invalid second prefix char 'I'
    [InlineData("AQ123456D")]        // Invalid second prefix char 'Q'
    [InlineData("AU123456A")]        // Invalid second prefix char 'U'
    [InlineData("AV123456B")]        // Invalid second prefix char 'V'
    [InlineData("AO123456C")]        // Invalid second prefix char 'O'
    [InlineData("BG123456A")]        // Invalid prefix combo
    [InlineData("GB123456B")]        // Invalid prefix combo
    [InlineData("NK123456C")]        // Invalid prefix combo
    [InlineData("KN123456D")]        // Invalid prefix combo
    [InlineData("NT123456A")]        // Invalid prefix combo
    [InlineData("TN123456B")]        // Invalid prefix combo
    [InlineData("ZZ123456C")]        // Invalid prefix combo
    public void TryParse_ShouldFail_WithInvalidFormats(string? input)
    {
        // Act
        bool success = NationalInsuranceNumber.TryParse(input, null, out var nino);

        // Assert
        Assert.False(success);
        Assert.Equal(default, nino);
    }

    [Fact]
    public void Parse_ShouldSucceed_WithValidInput()
    {
        // Arrange
        const string validNino = "AB123456C";

        // Act
        var nino = NationalInsuranceNumber.Parse(validNino);

        // Assert
        Assert.Equal(validNino, nino.ToString());
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("BG123456D")]
    public void Parse_ShouldThrowFormatException_WithInvalidInput(string input)
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => NationalInsuranceNumber.Parse(input));
    }

    [Fact]
    public void Parse_ShouldThrowFormatException_WithNullInput()
    {
        // Act & Assert
        Assert.Throws<FormatException>(() => NationalInsuranceNumber.Parse(null!));
    }


    // =================================================================================
    // Formatting Tests
    // =================================================================================

    [Fact]
    public void ToString_Default_ShouldReturnGeneralFormat()
    {
        // Arrange
        var nino = NationalInsuranceNumber.Parse("AB123456C");
        const string expected = "AB123456C";

        // Act
        string result = nino.ToString();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("G", "AB123456C")]
    [InlineData("g", "AB123456C")] // Case-insensitive
    [InlineData(null, "AB123456C")]
    public void ToString_WithFormat_ShouldReturnGeneralFormat(string? format, string expected)
    {
        // Arrange
        var nino = NationalInsuranceNumber.Parse("AB123456C");

        // Act
        string result = nino.ToString(format);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("S", "AB 12 34 56 C")]
    [InlineData("s", "AB 12 34 56 C")] // Case-insensitive
    public void ToString_WithFormat_ShouldReturnSpacedFormat(string format, string expected)
    {
        // Arrange
        var nino = NationalInsuranceNumber.Parse("AB123456C");

        // Act
        string result = nino.ToString(format);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToString_ShouldThrowFormatException_WithInvalidFormat()
    {
        // Arrange
        var nino = NationalInsuranceNumber.Parse("AB123456C");

        // Act & Assert
        Assert.Throws<FormatException>(() => nino.ToString("X"));
    }

    [Fact]
    public void TryFormat_ShouldSucceed_WithSufficientSpan()
    {
        // Arrange
        var nino = NationalInsuranceNumber.Parse("CE191295D");
        Span<char> destination = new char[13];

        // Act
        bool successG = nino.TryFormat(destination, out int charsWrittenG, "G", null);
        string resultG = new string(destination[..charsWrittenG]);

        bool successS = nino.TryFormat(destination, out int charsWrittenS, "S", null);
        string resultS = new string(destination[..charsWrittenS]);

        // Assert
        Assert.True(successG);
        Assert.Equal(9, charsWrittenG);
        Assert.Equal("CE191295D", resultG);

        Assert.True(successS);
        Assert.Equal(13, charsWrittenS);
        Assert.Equal("CE 19 12 95 D", resultS);
    }

    [Fact]
    public void TryFormat_ShouldFail_WithInsufficientSpan()
    {
        // Arrange
        var nino = NationalInsuranceNumber.Parse("CE191295D");
        Span<char> destination = new char[8];

        // Act
        bool success = nino.TryFormat(destination, out int charsWritten, "G", null);

        // Assert
        Assert.False(success);
        Assert.Equal(0, charsWritten);
    }

    [Fact]
    public void TryFormat_ShouldFail_WithInvalidFormat()
    {
        // Arrange
        var nino = NationalInsuranceNumber.Parse("CE191295D");
        Span<char> destination = new char[13];

        // Act
        bool success = nino.TryFormat(destination, out int charsWritten, "X", null);

        // Assert
        Assert.False(success);
        Assert.Equal(0, charsWritten);
    }


    // =================================================================================
    // Equality and Comparison Tests
    // =================================================================================

    [Fact]
    public void Equals_ShouldBeTrue_ForIdenticalNinos()
    {
        // Arrange
        var nino1 = NationalInsuranceNumber.Parse("QQ123456C");
        var nino2 = NationalInsuranceNumber.Parse("QQ 12 34 56 C");

        // Act & Assert
        Assert.True(nino1.Equals(nino2));
        Assert.True(nino1 == nino2);
        Assert.False(nino1 != nino2);
    }

    [Fact]
    public void Equals_ShouldBeFalse_ForDifferentNinos()
    {
        // Arrange
        var nino1 = NationalInsuranceNumber.Parse("AB123456C");
        var nino2 = NationalInsuranceNumber.Parse("AB123456D");

        // Act & Assert
        Assert.False(nino1.Equals(nino2));
        Assert.False(nino1 == nino2);
        Assert.True(nino1 != nino2);
    }

    [Fact]
    public void Equals_ShouldBeFalse_ForDifferentObjectTypes()
    {
        // Arrange
        var nino = NationalInsuranceNumber.Parse("AB123456C");
        var other = new object();

        // Act & Assert
        // ReSharper disable once SuspiciousTypeConversion.Global
        Assert.False(nino.Equals(other));
    }


    // =================================================================================
    // Hashing Tests
    // =================================================================================

    [Fact]
    public void GetHashCode_ShouldBeSame_ForEqualObjects()
    {
        // Arrange
        var nino1 = NationalInsuranceNumber.Parse("ZY987654A");
        var nino2 = NationalInsuranceNumber.Parse("zy 98 76 54 a");

        // Act & Assert
        Assert.Equal(nino1.GetHashCode(), nino2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_ShouldBeDifferent_ForDifferentObjects()
    {
        // Arrange
        var nino1 = NationalInsuranceNumber.Parse("ZY987654A");
        var nino2 = NationalInsuranceNumber.Parse("ZY987654B");

        // Act & Assert
        Assert.NotEqual(nino1.GetHashCode(), nino2.GetHashCode());
    }

    // =================================================================================
    // Casting Tests
    // =================================================================================

    [Fact]
    public void ExplicitCast_ToUlongAndBack_ShouldRoundtripSuccessfully()
    {
        // Arrange
        var originalNino = NationalInsuranceNumber.Parse("PY426719B");

        // Act
        ulong ulongValue = (ulong)originalNino;
        var roundtrippedNino = (NationalInsuranceNumber)ulongValue;

        // Assert
        Assert.Equal(originalNino, roundtrippedNino);
    }
}
