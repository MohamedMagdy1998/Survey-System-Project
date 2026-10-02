using Application.DTOs.Requests.Rolls;
using Application.Validators.Rolls;
using Xunit;

namespace Application.UnitTests.Validators.Rolls;

public class RoleRequestValidatorTests
{
    private readonly RoleRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenRequestIsValid_ReturnsSuccess()
    {
        // Arrange
        var request = new RoleRequest("Administrator", new List<string> { "polls:read", "polls:add" });

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public void Validate_WhenNameIsTooShortOrEmpty_ReturnsError(string name)
    {
        // Arrange
        var request = new RoleRequest(name, new List<string> { "polls:read" });

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RoleRequest.Name));
    }

    [Fact]
    public void Validate_WhenPermissionsIsEmpty_ReturnsError()
    {
        // Arrange
        var request = new RoleRequest("Admin", new List<string>());

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RoleRequest.Permissions));
    }

    [Fact]
    public void Validate_WhenPermissionsContainsDuplicates_ReturnsError()
    {
        // Arrange
        var request = new RoleRequest("Admin", new List<string> { "polls:read", "polls:read" });

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RoleRequest.Permissions));
    }
}
