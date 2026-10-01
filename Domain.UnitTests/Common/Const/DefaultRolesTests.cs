using Domain.Common.Const;
using Xunit;

namespace Domain.UnitTests.Common.Const;

public class DefaultRolesTests
{
    [Fact]
    public void Admin_WhenAccessed_ReturnsAdminRoleName()
    {
        // Act & Assert
        Assert.Equal("Admin", DefaultRoles.Admin);
    }

    [Fact]
    public void AdminRoleId_WhenAccessed_ReturnsExpectedGuidString()
    {
        // Act & Assert
        Assert.Equal("92b75286-d8f8-4061-9995-e6e23ccdee94", DefaultRoles.AdminRoleId);
    }

    [Fact]
    public void AdminRoleConcurrencyStamp_WhenAccessed_ReturnsExpectedGuidString()
    {
        // Act & Assert
        Assert.Equal("f51e5a91-bced-49c2-8b86-c2e170c0846c", DefaultRoles.AdminRoleConcurrencyStamp);
    }

    [Fact]
    public void Member_WhenAccessed_ReturnsMemberRoleName()
    {
        // Act & Assert
        Assert.Equal("Member", DefaultRoles.Member);
    }

    [Fact]
    public void MemberRoleId_WhenAccessed_ReturnsExpectedGuidString()
    {
        // Act & Assert
        Assert.Equal("9eaa03df-8e4f-4161-85de-0f6e5e30bfd4", DefaultRoles.MemberRoleId);
    }

    [Fact]
    public void MemberRoleConcurrencyStamp_WhenAccessed_ReturnsExpectedGuidString()
    {
        // Act & Assert
        Assert.Equal("5ee6bc12-5cb0-4304-91e7-6a00744e042a", DefaultRoles.MemberRoleConcurrencyStamp);
    }
}
