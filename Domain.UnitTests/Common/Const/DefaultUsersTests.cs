using Domain.Common.Const;
using Xunit;

namespace Domain.UnitTests.Common.Const;

public class DefaultUsersTests
{
    [Fact]
    public void AdminId_WhenAccessed_ReturnsExpectedGuidString()
    {
        // Act & Assert
        Assert.Equal("6dc6528a-b280-4770-9eae-82671ee81ef7", DefaultUsers.AdminId);
    }

    [Fact]
    public void AdminEmail_WhenAccessed_ReturnsExpectedEmailString()
    {
        // Act & Assert
        Assert.Equal("admin@survey-basket.com", DefaultUsers.AdminEmail);
    }

    [Fact]
    public void AdminPassword_WhenAccessed_ReturnsExpectedPasswordString()
    {
        // Act & Assert
        Assert.Equal("P@ssword123", DefaultUsers.AdminPassword);
    }

    [Fact]
    public void AdminSecurityStamp_WhenAccessed_ReturnsExpectedSecurityStampString()
    {
        // Act & Assert
        Assert.Equal("55BF92C9EF0249CDA210D85D1A851BC9", DefaultUsers.AdminSecurityStamp);
    }

    [Fact]
    public void AdminConcurrencyStamp_WhenAccessed_ReturnsExpectedConcurrencyStampString()
    {
        // Act & Assert
        Assert.Equal("99d2bbc6-bc54-4248-a172-a77de3ae4430", DefaultUsers.AdminConcurrencyStamp);
    }
}
