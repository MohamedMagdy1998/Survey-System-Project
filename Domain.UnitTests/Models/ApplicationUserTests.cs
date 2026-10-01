using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace Domain.UnitTests.Models;

public class ApplicationUserTests
{
    [Fact]
    public void Constructor_DefaultValues_InitializesDefaultsAndEmptyCollection()
    {
        // Act
        var user = new ApplicationUser();

        // Assert
        Assert.Equal(string.Empty, user.FirstName);
        Assert.Equal(string.Empty, user.LastName);
        Assert.False(user.IsDisabled);
        Assert.NotNull(user.RefreshTokens);
        Assert.Empty(user.RefreshTokens);
    }

    [Fact]
    public void Inheritance_WhenInstantiated_DerivesFromIdentityUser()
    {
        // Act
        var user = new ApplicationUser();

        // Assert
        Assert.IsAssignableFrom<IdentityUser>(user);
    }

    [Fact]
    public void Properties_WhenAssigned_StoreAssignedValues()
    {
        // Arrange
        var user = new ApplicationUser();
        var refreshTokens = new List<RefreshToken> { new() { Token = "token-1" } };

        // Act
        user.FirstName = "John";
        user.LastName = "Doe";
        user.IsDisabled = true;
        user.Email = "john.doe@example.com";
        user.UserName = "johndoe";
        user.RefreshTokens = refreshTokens;

        // Assert
        Assert.Equal("John", user.FirstName);
        Assert.Equal("Doe", user.LastName);
        Assert.True(user.IsDisabled);
        Assert.Equal("john.doe@example.com", user.Email);
        Assert.Equal("johndoe", user.UserName);
        Assert.Same(refreshTokens, user.RefreshTokens);
    }
}
