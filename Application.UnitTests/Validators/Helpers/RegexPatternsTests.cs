using Application.Validators.Helpers;
using System.Text.RegularExpressions;
using Xunit;

namespace Application.UnitTests.Validators.Helpers;

public class RegexPatternsTests
{
    [Theory]
    [InlineData("P@ssword1", true)]
    [InlineData("Strong#Pass99", true)]
    [InlineData("Valid1$Pass", true)]
    [InlineData("short1!", false)] // Less than 8 chars
    [InlineData("NoDigitAtAll!", false)] // Missing digit
    [InlineData("nolowercase123!", false)] // Missing lowercase
    [InlineData("NOUPPERCASE123!", false)] // Missing uppercase
    [InlineData("NoSpecialChar123", false)] // Missing special char
    public void PasswordRegex_WhenEvaluated_MatchesExpectedValidity(string password, bool expectedResult)
    {
        // Act
        var isMatch = Regex.IsMatch(password, RegexPatterns.Password);

        // Assert
        Assert.Equal(expectedResult, isMatch);
    }
}
