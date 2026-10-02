using Application.DTOs.Requests.Authorization;
using Application.Validators.Authorization;
using Xunit;

namespace Application.UnitTests.Validators.Authorization;

public class ResetPasswordRequestValidatorTests
{
    private readonly ResetPasswordRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenRequestIsValid_ReturnsSuccess()
    {
        // Arrange
        var request = new ResetPasswordRequest("user@example.com", "valid-code", "P@ssword123!");

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_WhenEmailIsInvalid_ReturnsError(string email)
    {
        // Arrange
        var request = new ResetPasswordRequest(email, "valid-code", "P@ssword123!");

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResetPasswordRequest.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenCodeIsEmpty_ReturnsError(string? code)
    {
        // Arrange
        var request = new ResetPasswordRequest("user@example.com", code!, "P@ssword123!");

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResetPasswordRequest.Code));
    }

    [Theory]
    [InlineData("")]
    [InlineData("short1!")]
    [InlineData("nouppercase123!")]
    public void Validate_WhenNewPasswordIsInvalid_ReturnsError(string newPassword)
    {
        // Arrange
        var request = new ResetPasswordRequest("user@example.com", "valid-code", newPassword);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ResetPasswordRequest.NewPassword));
    }
}
