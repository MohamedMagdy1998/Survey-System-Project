using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace Domain.UnitTests.Models;

public class ApplicationRoleTests
{
    [Fact]
    public void Constructor_DefaultValues_InitializesDefaults()
    {
        // Act
        var role = new ApplicationRole();

        // Assert
        Assert.False(role.IsDefault);
        Assert.False(role.IsDeleted);
    }

    [Fact]
    public void Inheritance_WhenInstantiated_DerivesFromIdentityRole()
    {
        // Act
        var role = new ApplicationRole();

        // Assert
        Assert.IsAssignableFrom<IdentityRole>(role);
    }

    [Fact]
    public void Properties_WhenAssigned_StoreAssignedValues()
    {
        // Arrange
        var role = new ApplicationRole();

        // Act
        role.Id = "role-id-123";
        role.Name = "CustomRole";
        role.IsDefault = true;
        role.IsDeleted = true;

        // Assert
        Assert.Equal("role-id-123", role.Id);
        Assert.Equal("CustomRole", role.Name);
        Assert.True(role.IsDefault);
        Assert.True(role.IsDeleted);
    }
}
