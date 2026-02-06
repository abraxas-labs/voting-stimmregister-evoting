// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using FluentAssertions;
using Voting.Stimmregister.EVoting.Core.Utils;
using Xunit;

namespace Voting.Stimmregister.EVoting.Core.Unit.Tests.UtilsTests;

public class RandomCodeGeneratorTests
{
    [Theory]
    [InlineData(1, 2)]
    [InlineData(8, 11)]
    [InlineData(16, 22)]
    [InlineData(32, 43)]
    public void GenerateBase64UrlSafeCode_ShouldReturnStringOfRequestedLength(int inputLength, int expectedCodeLength)
    {
        // Act
        var code = RandomCodeGenerator.GenerateBase64UrlSafeCode(inputLength);

        // Assert
        code.Should().NotBeNullOrEmpty();
        code.Length.Should().Be(expectedCodeLength);
    }

    [Fact]
    public void GenerateBase64UrlSafeCode_ShouldReturnUrlSafeCharacters()
    {
        // Arrange
        var length = 32;
        var code = RandomCodeGenerator.GenerateBase64UrlSafeCode(length);

        // Act
        var isUrlSafe = code.All(c =>
            (c >= 'A' && c <= 'Z') ||
            (c >= 'a' && c <= 'z') ||
            (c >= '0' && c <= '9') ||
            c == '-' ||
            c == '_');

        // Assert
        isUrlSafe.Should().BeTrue("code should only contain URL-safe Base64 characters");
    }

    [Fact]
    public void GenerateBase64UrlSafeCode_ShouldNotContainPadding()
    {
        // Arrange
        var length = 24;
        var code = RandomCodeGenerator.GenerateBase64UrlSafeCode(length);

        // Assert
        code.Should().NotContain("=");
    }

    [Fact]
    public void GenerateBase64UrlSafeCode_ShouldReturnDifferentResults()
    {
        // Arrange
        var length = 16;

        // Act
        var code1 = RandomCodeGenerator.GenerateBase64UrlSafeCode(length);
        var code2 = RandomCodeGenerator.GenerateBase64UrlSafeCode(length);

        // Assert
        code1.Should().NotBe(code2, "two generated codes should be different");
    }

    [Fact]
    public void GenerateBase64UrlSafeCode_LengthLessThanOrEqualZero_ShouldThrow()
    {
        // Arrange
        Action actZero = () => RandomCodeGenerator.GenerateBase64UrlSafeCode(0);
        Action actNegative = () => RandomCodeGenerator.GenerateBase64UrlSafeCode(-1);

        // Assert
        actZero.Should().Throw<ArgumentOutOfRangeException>();
        actNegative.Should().Throw<ArgumentOutOfRangeException>();
    }
}
