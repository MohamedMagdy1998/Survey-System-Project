using Domain.Models;
using Xunit;

namespace Domain.UnitTests.Models;

public class RefreshTokenTests
{
    [Fact]
    public void Constructor_DefaultValues_InitializesDefaultState()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var token = new RefreshToken();
        var after = DateTime.UtcNow;

        // Assert
        Assert.Equal(string.Empty, token.Token);
        Assert.Equal(default, token.ExpiresOn);
        Assert.Null(token.RevokedOn);
        Assert.True(token.CreatedOn >= before && token.CreatedOn <= after);
    }

    [Fact]
    public void IsExpired_WhenExpiresOnIsInFuture_ReturnsFalse()
    {
        // Arrange
        var token = new RefreshToken
        {
            ExpiresOn = DateTime.UtcNow.AddDays(7)
        };

        // Act & Assert
        Assert.False(token.IsExpired);
    }

    [Fact]
    public void IsExpired_WhenExpiresOnIsInPast_ReturnsTrue()
    {
        // Arrange
        var token = new RefreshToken
        {
            ExpiresOn = DateTime.UtcNow.AddMinutes(-5)
        };

        // Act & Assert
        Assert.True(token.IsExpired);
    }

    [Fact]
    public void IsActive_WhenRevokedOnIsNullAndNotExpired_ReturnsTrue()
    {
        // Arrange
        var token = new RefreshToken
        {
            ExpiresOn = DateTime.UtcNow.AddDays(1),
            RevokedOn = null
        };

        // Act & Assert
        Assert.True(token.IsActive);
    }

    [Fact]
    public void IsActive_WhenRevokedOnIsNotNullAndNotExpired_ReturnsFalse()
    {
        // Arrange
        var token = new RefreshToken
        {
            ExpiresOn = DateTime.UtcNow.AddDays(1),
            RevokedOn = DateTime.UtcNow
        };

        // Act & Assert
        Assert.False(token.IsActive);
    }

    [Fact]
    public void IsActive_WhenRevokedOnIsNullAndExpired_ReturnsFalse()
    {
        // Arrange
        var token = new RefreshToken
        {
            ExpiresOn = DateTime.UtcNow.AddHours(-1),
            RevokedOn = null
        };

        // Act & Assert
        Assert.False(token.IsActive);
    }

    [Fact]
    public void IsActive_WhenRevokedOnIsNotNullAndExpired_ReturnsFalse()
    {
        // Arrange
        var token = new RefreshToken
        {
            ExpiresOn = DateTime.UtcNow.AddHours(-1),
            RevokedOn = DateTime.UtcNow.AddHours(-2)
        };

        // Act & Assert
        Assert.False(token.IsActive);
    }

    [Fact]
    public void Properties_WhenAssigned_StoreAssignedValues()
    {
        // Arrange
        var token = new RefreshToken();
        var created = DateTime.UtcNow.AddDays(-1);
        var expires = DateTime.UtcNow.AddDays(6);
        var revoked = DateTime.UtcNow;

        // Act
        token.Token = "sample-refresh-token";
        token.CreatedOn = created;
        token.ExpiresOn = expires;
        token.RevokedOn = revoked;

        // Assert
        Assert.Equal("sample-refresh-token", token.Token);
        Assert.Equal(created, token.CreatedOn);
        Assert.Equal(expires, token.ExpiresOn);
        Assert.Equal(revoked, token.RevokedOn);
    }
}
