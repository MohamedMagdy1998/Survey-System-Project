using Application.DTOs.Requests.Authorization;
using Application.Validators.Authorization;
using Xunit;

namespace Application.UnitTests.Validators.Authorization;

public class ConfirmEmailRequestValidatorTests
{
    private readonly ConfirmEmailRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenRequestIsValid_ReturnsSuccess()
    {
        // Arrange
        var request = new ConfirmEmailRequest("user-1", "code-xyz");

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WhenUserIdIsMissing_ReturnsError(string? userId)
    {
        // Arrange
        var request = new ConfirmEmailRequest(userId!, "code-xyz");

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ConfirmEmailRequest.UserId));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WhenCodeIsMissing_ReturnsError(string? code)
    {
        // Arrange
        var request = new ConfirmEmailRequest("user-1", code!);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ConfirmEmailRequest.Code));
    }
}
