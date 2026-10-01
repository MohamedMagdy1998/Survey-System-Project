using Domain.Common.Const;
using Xunit;

namespace Domain.UnitTests.Common.Const;

public class PermissionsTests
{
    [Fact]
    public void Type_WhenAccessed_ReturnsPermissionsString()
    {
        // Act & Assert
        Assert.Equal("permissions", Permissions.Type);
    }

    [Fact]
    public void PollPermissionsConstants_WhenAccessed_HaveExpectedValues()
    {
        // Act & Assert
        Assert.Equal("polls:read", Permissions.GetPolls);
        Assert.Equal("polls:add", Permissions.AddPolls);
        Assert.Equal("polls:update", Permissions.UpdatePolls);
        Assert.Equal("polls:delete", Permissions.DeletePolls);
    }

    [Fact]
    public void QuestionPermissionsConstants_WhenAccessed_HaveExpectedValues()
    {
        // Act & Assert
        Assert.Equal("questions:read", Permissions.GetQuestions);
        Assert.Equal("questions:add", Permissions.AddQuestions);
        Assert.Equal("questions:update", Permissions.UpdateQuestions);
    }

    [Fact]
    public void UserPermissionsConstants_WhenAccessed_HaveExpectedValues()
    {
        // Act & Assert
        Assert.Equal("users:read", Permissions.GetUsers);
        Assert.Equal("users:add", Permissions.AddUsers);
        Assert.Equal("users:update", Permissions.UpdateUsers);
    }

    [Fact]
    public void RolePermissionsConstants_WhenAccessed_HaveExpectedValues()
    {
        // Act & Assert
        Assert.Equal("roles:read", Permissions.GetRoles);
        Assert.Equal("roles:add", Permissions.AddRoles);
        Assert.Equal("roles:update", Permissions.UpdateRoles);
    }

    [Fact]
    public void ResultsPermissionsConstant_WhenAccessed_HasExpectedValue()
    {
        // Act & Assert
        Assert.Equal("results:read", Permissions.Results);
    }

    [Fact]
    public void GetAllPermissions_WhenCalled_ReturnsAll14Permissions()
    {
        // Act
        var permissions = Permissions.GetAllPermissions();

        // Assert
        Assert.NotNull(permissions);
        Assert.Equal(14, permissions.Count);
        Assert.DoesNotContain(null, permissions);
        Assert.DoesNotContain(string.Empty, permissions);
    }

    [Fact]
    public void GetAllPermissions_WhenCalled_ContainsAllExpectedPermissionValues()
    {
        // Arrange
        var expectedPermissions = new[]
        {
            Permissions.GetPolls,
            Permissions.AddPolls,
            Permissions.UpdatePolls,
            Permissions.DeletePolls,
            Permissions.GetQuestions,
            Permissions.AddQuestions,
            Permissions.UpdateQuestions,
            Permissions.GetUsers,
            Permissions.AddUsers,
            Permissions.UpdateUsers,
            Permissions.GetRoles,
            Permissions.AddRoles,
            Permissions.UpdateRoles,
            Permissions.Results
        };

        // Act
        var permissions = Permissions.GetAllPermissions();

        // Assert
        foreach (var expected in expectedPermissions)
        {
            Assert.Contains(expected, permissions);
        }
    }
}
