using Application.DTOs.Requests.Users;
using Application.Validators.Users;
using Xunit;

namespace Application.UnitTests.Validators.Users;

public class ChangePasswordRequestValidatorTests
{
    private readonly ChangePasswordRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenRequestIsValid_ReturnsSuccess()
    {
        // Arrange
        var request = new ChangePasswordRequest("OldP@ssword1", "NewP@ssword2");

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenCurrentPasswordIsEmpty_ReturnsError(string? currentPassword)
    {
        // Arrange
        var request = new ChangePasswordRequest(currentPassword!, "NewP@ssword2");

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ChangePasswordRequest.CurrentPassword));
    }

    [Fact]
    public void Validate_WhenNewPasswordIsSameAsCurrent_ReturnsError()
    {
        // Arrange
        var request = new ChangePasswordRequest("SameP@ssword1", "SameP@ssword1");

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ChangePasswordRequest.NewPassword));
    }

    [Theory]
    [InlineData("short1!")]
    [InlineData("password123!")]
    [InlineData("PASSWORD123!")]
    public void Validate_WhenNewPasswordDoesNotMeetRequirements_ReturnsError(string newPassword)
    {
        // Arrange
        var request = new ChangePasswordRequest("OldP@ssword1", newPassword);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ChangePasswordRequest.NewPassword));
    }
}
