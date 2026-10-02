using Application.DTOs.Requests.Users;
using Application.Validators.Users;
using Xunit;

namespace Application.UnitTests.Validators.Users;

public class UpdateProfileRequestValidatorTests
{
    private readonly UpdateProfileRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenRequestIsValid_ReturnsSuccess()
    {
        // Arrange
        var request = new UpdateProfileRequest("Jane", "Doe");

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public void Validate_WhenFirstNameIsInvalid_ReturnsError(string firstName)
    {
        // Arrange
        var request = new UpdateProfileRequest(firstName, "Doe");

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProfileRequest.FirstName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public void Validate_WhenLastNameIsInvalid_ReturnsError(string lastName)
    {
        // Arrange
        var request = new UpdateProfileRequest("Jane", lastName);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateProfileRequest.LastName));
    }
}
