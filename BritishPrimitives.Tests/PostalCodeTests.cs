using System.Diagnostics.CodeAnalysis;

namespace BritishPrimitives.Tests;

[ExcludeFromCodeCoverage]
public sealed class PostalCodeTests
{
    [Theory]
    [InlineData("M1 1AA")]
    [InlineData("m1 1aa")]
    [InlineData("M60 1NW")]
    [InlineData("CR2 6XH")]
    [InlineData("DN55 1PT")]
    [InlineData("W1A 1HQ")] // ANA NAA format
    [InlineData("EC1A 1BB")]
    [InlineData("SW1A 0AA")] // Houses of Parliament
    [InlineData("GIR 0AA")] // Girobank
    [InlineData("sw1a0aa")]
    [InlineData("Sw1A 0aA")]
    public void TryParse_WithValidPostcodes_ShouldSucceed(string postcodeString)
    {
        // Act
        var result = PostalCode.TryParse(postcodeString, null, out var postcode);

        // Assert
        Assert.True(result);
        Assert.False(string.IsNullOrEmpty(postcode.OutwardCode));
        Assert.False(string.IsNullOrEmpty(postcode.InwardCode));
    }

    [Theory]
    [InlineData("Z1 1AA")]  // Invalid first letter
    [InlineData("MAA 1AA")] // Invalid format
    [InlineData("M1 1A")]   // Too short
    [InlineData("M1 1AAA")] // Too long
    [InlineData("M1 1AC")]  // Invalid inward char 'C'
    [InlineData("M1 1AI")]  // Invalid inward char 'I'
    [InlineData("M1 1AK")]  // Invalid inward char 'K'
    [InlineData("M1 1AM")]  // Invalid inward char 'M'
    [InlineData("M1 1AO")]  // Invalid inward char 'O'
    [InlineData("M1 1AV")]  // Invalid inward char 'V'
    [InlineData("1M 1AA")]  // Invalid start
    [InlineData(" ABC ")]
    [InlineData("")]
    [InlineData(null)]
    public void TryParse_WithInvalidPostcodes_ShouldFail(string? postcodeString)
    {
        // Act
        var result = PostalCode.TryParse(postcodeString, null, out _);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TryFormat_WithShorthandFormat_ShouldReturnPostcodeWithoutSpace()
    {
        // Arrange
        PostalCode.TryParse("SW1A 0AA", null, out var postcode);
        Span<char> destination = new char[10];

        // Act
        var result = postcode.TryFormat(destination, out int charsWritten, "s", null);

        // Assert
        Assert.True(result);
        Assert.Equal("SW1A0AA".Length, charsWritten);
        Assert.Equal("SW1A0AA", destination[..charsWritten].ToString());
    }

    [Fact]
    public void TryFormat_WithUppercaseShorthandFormat_ShouldReturnPostcodeWithoutSpace()
    {
        // Arrange
        PostalCode.TryParse("EC1A 1BB", null, out var postcode);
        Span<char> destination = new char[10];

        // Act
        var result = postcode.TryFormat(destination, out int charsWritten, "S", null);

        // Assert
        Assert.True(result);
        Assert.Equal("EC1A1BB".Length, charsWritten);
        Assert.Equal("EC1A1BB", destination[..charsWritten].ToString());
    }

    [Fact]
    public void TryFormat_WithDefaultFormat_ShouldReturnPostcodeWithSpace()
    {
        // Arrange
        PostalCode.TryParse("M60 1NW", null, out var postcode);
        Span<char> destination = new char[10];

        // Act
        var result = postcode.TryFormat(destination, out int charsWritten, "", null);

        // Assert
        Assert.True(result);
        Assert.Equal("M60 1NW".Length, charsWritten);
        Assert.Equal("M60 1NW", destination[..charsWritten].ToString());
    }

    [Fact]
    public void TryFormat_WithNullFormat_ShouldReturnPostcodeWithSpace()
    {
        // Arrange
        PostalCode.TryParse("M60 1NW", null, out var postcode);
        Span<char> destination = new char[10];

        // Act
        var result = postcode.TryFormat(destination, out int charsWritten, null, null);

        // Assert
        Assert.True(result);
        Assert.Equal("M60 1NW".Length, charsWritten);
        Assert.Equal("M60 1NW", destination[..charsWritten].ToString());
    }


    [Fact]
    public void TryFormat_WithInsufficientSpace_ShouldFail()
    {
        // Arrange
        PostalCode.TryParse("CR2 6XH", null, out var postcode);
        Span<char> destination = new char[5];

        // Act
        var result = postcode.TryFormat(destination, out int charsWritten, "", null);

        // Assert
        Assert.False(result);
        Assert.Equal(0, charsWritten);
    }

    [Fact]
    public void Equals_WithTwoEqualPostcodes_ShouldReturnTrue()
    {
        // Arrange
        PostalCode.TryParse("DN55 1PT", null, out var postcode1);
        PostalCode.TryParse("dn55 1pt", null, out var postcode2);

        // Assert
        Assert.True(postcode1.Equals(postcode2));
        Assert.True(postcode1 == postcode2);
        Assert.False(postcode1 != postcode2);
    }

    [Fact]
    public void Equals_WithTwoDifferentPostcodes_ShouldReturnFalse()
    {
        // Arrange
        PostalCode.TryParse("M1 1AA", null, out var postcode1);
        PostalCode.TryParse("M1 1AB", null, out var postcode2);

        // Assert
        Assert.False(postcode1.Equals(postcode2));
        Assert.False(postcode1 == postcode2);
        Assert.True(postcode1 != postcode2);
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        // Arrange
        PostalCode.TryParse("PO1 3AX", null, out var postcode);

        // Act
        var result = postcode.ToString();

        // Assert
        Assert.Equal("PO1 3AX", result);
    }
}