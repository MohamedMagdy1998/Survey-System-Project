using Application.DTOs.Requests.Users;
using Application.Validators.Users;
using Xunit;

namespace Application.UnitTests.Validators.Users;

public class CreateUserRequestValidatorTests
{
    private readonly CreateUserRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenRequestIsValid_ReturnsSuccess()
    {
        // Arrange
        var request = new CreateUserRequest("Jane", "Doe", "jane@example.com", "P@ssword123!", new List<string> { "Member" });

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
        var request = new CreateUserRequest("Jane", "Doe", email, "P@ssword123!", new List<string> { "Member" });

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserRequest.Email));
    }

    [Theory]
    [InlineData("short1!")]
    [InlineData("password123!")]
    public void Validate_WhenPasswordIsInvalid_ReturnsError(string password)
    {
        // Arrange
        var request = new CreateUserRequest("Jane", "Doe", "jane@example.com", password, new List<string> { "Member" });

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserRequest.Password));
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public void Validate_WhenFirstNameIsInvalid_ReturnsError(string firstName)
    {
        // Arrange
        var request = new CreateUserRequest(firstName, "Doe", "jane@example.com", "P@ssword123!", new List<string> { "Member" });

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserRequest.FirstName));
    }

    [Fact]
    public void Validate_WhenRolesContainsDuplicates_ReturnsError()
    {
        // Arrange
        var request = new CreateUserRequest("Jane", "Doe", "jane@example.com", "P@ssword123!", new List<string> { "Member", "Member" });

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserRequest.Roles));
    }
}
