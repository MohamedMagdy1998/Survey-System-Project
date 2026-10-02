using Application.DTOs.Requests.Rolls;
using Application.Services_Implementations;
using Domain.Common.Abstractions.Errors;
using Domain.Common.Const;
using Domain.Contracts;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Application.UnitTests.Common.TestHelpers;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Application.UnitTests.Services_Implementations;

public class RoleServiceTests
{
    private readonly Mock<RoleManager<ApplicationRole>> _roleManagerMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly RoleService _sut;

    public RoleServiceTests()
    {
        var roleStore = new Mock<IRoleStore<ApplicationRole>>();
        _roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStore.Object,
            new IRoleValidator<ApplicationRole>[0],
            new Mock<ILookupNormalizer>().Object,
            new IdentityErrorDescriber(),
            new Mock<ILogger<RoleManager<ApplicationRole>>>().Object);

        var roleRepositoryMock = new Mock<IRoleRepository>();
        _unitOfWorkMock.Setup(u => u.Roles).Returns(roleRepositoryMock.Object);

        _sut = new RoleService(_roleManagerMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_WhenCalled_ReturnsNonDefaultAndActiveRoles()
    {
        // Arrange
        var roles = new List<ApplicationRole>
        {
            new() { Id = "1", Name = "Admin", IsDefault = false, IsDeleted = false },
            new() { Id = "2", Name = "DefaultRole", IsDefault = true, IsDeleted = false },
            new() { Id = "3", Name = "DeletedRole", IsDefault = false, IsDeleted = true }
        }.BuildMock();

        _roleManagerMock.Setup(r => r.Roles).Returns(roles);

        // Act
        var result = await _sut.GetAllAsync(includeDisabled: false);

        // Assert
        var singleRole = Assert.Single(result);
        Assert.Equal("Admin", singleRole.Name);
    }

    [Fact]
    public async Task GetAsync_WhenRoleNotFound_ReturnsRoleNotFoundError()
    {
        // Arrange
        _roleManagerMock.Setup(r => r.FindByIdAsync("non-existent"))
            .ReturnsAsync((ApplicationRole?)null);

        // Act
        var result = await _sut.GetAsync("non-existent");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RoleErrors.RoleNotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task GetAsync_WhenRoleExists_ReturnsRoleDetailResponseWithPermissions()
    {
        // Arrange
        var role = new ApplicationRole { Id = "role-1", Name = "Manager", IsDeleted = false };
        var claims = new List<Claim> { new(Permissions.Type, Permissions.Results) };

        _roleManagerMock.Setup(r => r.FindByIdAsync("role-1"))
            .ReturnsAsync(role);
        _roleManagerMock.Setup(r => r.GetClaimsAsync(role))
            .ReturnsAsync(claims);

        // Act
        var result = await _sut.GetAsync("role-1");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Manager", result.Value.Name);
        Assert.Contains(Permissions.Results, result.Value.Permissions);
    }

    [Fact]
    public async Task AddAsync_WhenRoleAlreadyExists_ReturnsDuplicatedRoleError()
    {
        // Arrange
        var request = new RoleRequest("Admin", new List<string> { Permissions.GetPolls });
        _roleManagerMock.Setup(r => r.RoleExistsAsync("Admin"))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RoleErrors.DuplicatedRole.Code, result.Error.Code);
    }

    [Fact]
    public async Task AddAsync_WhenPermissionsAreInvalid_ReturnsInvalidPermissionsError()
    {
        // Arrange
        var request = new RoleRequest("NewRole", new List<string> { "invalid:permission" });
        _roleManagerMock.Setup(r => r.RoleExistsAsync("NewRole"))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RoleErrors.InvalidPermissions.Code, result.Error.Code);
    }

    [Fact]
    public async Task AddAsync_WhenRoleCreationFails_ReturnsBadRequestError()
    {
        // Arrange
        var request = new RoleRequest("NewRole", new List<string> { Permissions.GetPolls });
        _roleManagerMock.Setup(r => r.RoleExistsAsync("NewRole"))
            .ReturnsAsync(false);
        _roleManagerMock.Setup(r => r.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Failed to create" }));

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Error", result.Error.Code);
    }

    [Fact]
    public async Task AddAsync_WhenValidRequest_CreatesRoleAndAddsPermissions()
    {
        // Arrange
        var request = new RoleRequest("NewRole", new List<string> { Permissions.GetPolls });
        _roleManagerMock.Setup(r => r.RoleExistsAsync("NewRole"))
            .ReturnsAsync(false);
        _roleManagerMock.Setup(r => r.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("NewRole", result.Value.Name);
        _unitOfWorkMock.Verify(u => u.Roles.AddPermissionsAsync(It.IsAny<string>(), request.Permissions, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenRoleNameDuplicated_ReturnsDuplicatedRoleError()
    {
        // Arrange
        var request = new RoleRequest("ExistingName", new List<string> { Permissions.GetPolls });
        var roles = new List<ApplicationRole>
        {
            new() { Id = "other-role", Name = "ExistingName" }
        }.BuildMock();

        _roleManagerMock.Setup(r => r.Roles).Returns(roles);

        // Act
        var result = await _sut.UpdateAsync("current-role", request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RoleErrors.DuplicatedRole.Code, result.Error.Code);
    }

    [Fact]
    public async Task UpdateAsync_WhenRoleNotFound_ReturnsRoleNotFoundError()
    {
        // Arrange
        var request = new RoleRequest("UniqueName", new List<string> { Permissions.GetPolls });
        var roles = new List<ApplicationRole>().BuildMock();

        _roleManagerMock.Setup(r => r.Roles).Returns(roles);
        _roleManagerMock.Setup(r => r.FindByIdAsync("role-1"))
            .ReturnsAsync((ApplicationRole?)null);

        // Act
        var result = await _sut.UpdateAsync("role-1", request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RoleErrors.RoleNotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task UpdateAsync_WhenPermissionsAreInvalid_ReturnsInvalidPermissionsError()
    {
        // Arrange
        var request = new RoleRequest("UniqueName", new List<string> { "invalid:permission" });
        var roles = new List<ApplicationRole>().BuildMock();
        var role = new ApplicationRole { Id = "role-1", Name = "UniqueName" };

        _roleManagerMock.Setup(r => r.Roles).Returns(roles);
        _roleManagerMock.Setup(r => r.FindByIdAsync("role-1"))
            .ReturnsAsync(role);

        // Act
        var result = await _sut.UpdateAsync("role-1", request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RoleErrors.InvalidPermissions.Code, result.Error.Code);
    }

    [Fact]
    public async Task UpdateAsync_WhenValidRequest_UpdatesRoleAndPermissions()
    {
        // Arrange
        var request = new RoleRequest("UpdatedName", new List<string> { Permissions.GetPolls });
        var roles = new List<ApplicationRole>().BuildMock();
        var role = new ApplicationRole { Id = "role-1", Name = "OldName" };

        _roleManagerMock.Setup(r => r.Roles).Returns(roles);
        _roleManagerMock.Setup(r => r.FindByIdAsync("role-1"))
            .ReturnsAsync(role);
        _roleManagerMock.Setup(r => r.UpdateAsync(role))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.UpdateAsync("role-1", request);

        // Assert
        Assert.True(result.IsSuccess);
        _unitOfWorkMock.Verify(u => u.Roles.UpdatePermissionsAsync("role-1", request.Permissions, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ToggleStatusAsync_WhenRoleNotFound_ReturnsRoleNotFoundError()
    {
        // Arrange
        _roleManagerMock.Setup(r => r.FindByIdAsync("missing"))
            .ReturnsAsync((ApplicationRole?)null);

        // Act
        var result = await _sut.ToggleStatusAsync("missing");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RoleErrors.RoleNotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task ToggleStatusAsync_WhenRoleExists_TogglesStatusAndReturnsSuccess()
    {
        // Arrange
        var role = new ApplicationRole { Id = "role-1", IsDeleted = false };
        _roleManagerMock.Setup(r => r.FindByIdAsync("role-1"))
            .ReturnsAsync(role);
        _roleManagerMock.Setup(r => r.UpdateAsync(role))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.ToggleStatusAsync("role-1");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(role.IsDeleted);
    }
}
