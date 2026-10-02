using Application.DTOs.Requests.Authorization;
using Application.Validators.Authorization;
using Xunit;

namespace Application.UnitTests.Validators.Authorization;

public class ResendConfirmationEmailRequestValidatorTests
{
    private readonly ResendConfirmationEmailRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenEmailIsValid_ReturnsSuccess()
    {
        // Arrange
        var request = new ResendConfirmationEmailRequest("valid@example.com");

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenEmailIsEmpty_ReturnsError(string? email)
    {
        // Arrange
        var request = new ResendConfirmationEmailRequest(email!);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResendConfirmationEmailRequest.Email));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("user@")]
    public void Validate_WhenEmailIsInvalidFormat_ReturnsError(string email)
    {
        // Arrange
        var request = new ResendConfirmationEmailRequest(email);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResendConfirmationEmailRequest.Email));
    }
}
