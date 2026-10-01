using Domain.Common.Abstractions.Errors;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Domain.UnitTests.Common.Abstractions.Errors;

public class RoleErrorsTests
{
    [Fact]
    public void RoleNotFound_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = RoleErrors.RoleNotFound;

        // Assert
        Assert.Equal("Role.RoleNotFound", error.Code);
        Assert.Equal("Role is not found", error.Description);
        Assert.Equal(StatusCodes.Status404NotFound, error.StatusCode);
    }

    [Fact]
    public void InvalidPermissions_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = RoleErrors.InvalidPermissions;

        // Assert
        Assert.Equal("Role.InvalidPermissions", error.Code);
        Assert.Equal("Invalid permissions", error.Description);
        Assert.Equal(StatusCodes.Status400BadRequest, error.StatusCode);
    }

    [Fact]
    public void DuplicatedRole_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = RoleErrors.DuplicatedRole;

        // Assert
        Assert.Equal("Role.DuplicatedRole", error.Code);
        Assert.Equal("Another role with the same name is already exists", error.Description);
        Assert.Equal(StatusCodes.Status409Conflict, error.StatusCode);
    }
}
