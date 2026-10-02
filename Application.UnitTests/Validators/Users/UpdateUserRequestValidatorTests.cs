using Application.DTOs.Requests.Users;
using Application.Validators.Users;
using Xunit;

namespace Application.UnitTests.Validators.Users;

public class UpdateUserRequestValidatorTests
{
    private readonly UpdateUserRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenRequestIsValid_ReturnsSuccess()
    {
        // Arrange
        var request = new UpdateUserRequest("Jane", "Doe", "jane@example.com", new List<string> { "Member" });

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
        var request = new UpdateUserRequest("Jane", "Doe", email, new List<string> { "Member" });

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateUserRequest.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public void Validate_WhenFirstNameIsInvalid_ReturnsError(string firstName)
    {
        // Arrange
        var request = new UpdateUserRequest(firstName, "Doe", "jane@example.com", new List<string> { "Member" });

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateUserRequest.FirstName));
    }

    [Fact]
    public void Validate_WhenRolesContainsDuplicates_ReturnsError()
    {
        // Arrange
        var request = new UpdateUserRequest("Jane", "Doe", "jane@example.com", new List<string> { "Admin", "Admin" });

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateUserRequest.Roles));
    }
}
