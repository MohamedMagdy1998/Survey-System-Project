using Application.DTOs.Requests.Authorization;
using Application.Validators.Authorization;
using Xunit;

namespace Application.UnitTests.Validators.Authorization;

public class RefreshTokenRequestValidatorTests
{
    private readonly RefreshTokenRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenRequestIsValid_ReturnsSuccess()
    {
        // Arrange
        var request = new RefreshTokenRequest("valid-token", "valid-refresh-token");

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenTokenIsEmpty_ReturnsError(string? token)
    {
        // Arrange
        var request = new RefreshTokenRequest(token!, "valid-refresh-token");

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RefreshTokenRequest.Token));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenRefreshTokenIsEmpty_ReturnsError(string? refreshToken)
    {
        // Arrange
        var request = new RefreshTokenRequest("valid-token", refreshToken!);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RefreshTokenRequest.RefreshToken));
    }
}
