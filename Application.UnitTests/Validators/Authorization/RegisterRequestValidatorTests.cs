using Application.DTOs.Requests.Authorization;
using Application.Validators.Authorization;
using Xunit;

namespace Application.UnitTests.Validators.Authorization;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenRequestIsValid_ReturnsSuccess()
    {
        // Arrange
        var request = new RegisterRequest("user@example.com", "John", "Doe", "P@ssword123!");

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
        var request = new RegisterRequest(email, "John", "Doe", "P@ssword123!");

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.Email));
    }

    [Theory]
    [InlineData("ab")] // Less than 3 chars
    [InlineData("")]
    public void Validate_WhenFirstNameIsInvalid_ReturnsError(string firstName)
    {
        // Arrange
        var request = new RegisterRequest("user@example.com", firstName, "Doe", "P@ssword123!");

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.FirstName));
    }

    [Theory]
    [InlineData("ab")] // Less than 3 chars
    [InlineData("")]
    public void Validate_WhenLastNameIsInvalid_ReturnsError(string lastName)
    {
        // Arrange
        var request = new RegisterRequest("user@example.com", "John", lastName, "P@ssword123!");

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.LastName));
    }

    [Theory]
    [InlineData("short1!")] // < 8
    [InlineData("passwordwithoutdigits!")] // No digits or uppercase
    [InlineData("PASSWORD123!")] // No lowercase
    public void Validate_WhenPasswordDoesNotMeetRequirements_ReturnsError(string password)
    {
        // Arrange
        var request = new RegisterRequest("user@example.com", "John", "Doe", password);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RegisterRequest.Password));
    }
}
