using Application.DTOs.Requests.Users;
using Application.DTOs.Responses.Rolls;
using Application.DTOs.Responses.Users;
using Application.Services_Implementations;
using Application.Services_Interfaces;
using Domain.Common.Abstractions.Errors;
using Domain.Contracts;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Application.UnitTests.Common.TestHelpers;
using Moq;
using Xunit;

namespace Application.UnitTests.Services_Implementations;

public class UserServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IRoleService> _roleServiceMock = new();
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepositoryMock.Object);

        _sut = new UserService(_userManagerMock.Object, _roleServiceMock.Object, _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_WhenCalled_ReturnsUsersWithRoles()
    {
        // Arrange
        var user = new ApplicationUser { Id = "u1", FirstName = "John", LastName = "Doe", Email = "john@example.com", IsDisabled = false };
        var usersWithRoles = new List<(ApplicationUser User, IEnumerable<string> Roles)>
        {
            (user, new List<string> { "Member" })
        };

        _unitOfWorkMock.Setup(u => u.Users.GetAllUsersWithRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(usersWithRoles);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        var singleUser = Assert.Single(result);
        Assert.Equal("john@example.com", singleUser.Email);
        Assert.Contains("Member", singleUser.Roles);
    }

    [Fact]
    public async Task GetAsync_WhenUserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _userManagerMock.Setup(m => m.FindByIdAsync("non-existent"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.GetAsync("non-existent");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task GetAsync_WhenUserExists_ReturnsUserResponseWithRoles()
    {
        // Arrange
        var user = new ApplicationUser { Id = "u1", FirstName = "John", LastName = "Doe", Email = "john@example.com", IsDisabled = false };
        _userManagerMock.Setup(m => m.FindByIdAsync("u1"))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Admin" });

        // Act
        var result = await _sut.GetAsync("u1");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("u1", result.Value.Id);
        Assert.Contains("Admin", result.Value.Roles);
    }

    [Fact]
    public async Task AddAsync_WhenEmailAlreadyExists_ReturnsDuplicateEmailError()
    {
        // Arrange
        var request = new CreateUserRequest("Jane", "Doe", "existing@example.com", "P@ssword1", new List<string> { "Member" });
        var users = new List<ApplicationUser>
        {
            new() { Email = "existing@example.com" }
        }.BuildMock();

        _userManagerMock.Setup(m => m.Users).Returns(users);

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.DuplicateEmail.Code, result.Error.Code);
    }

    [Fact]
    public async Task AddAsync_WhenRoleIsInvalid_ReturnsInvalidCredentialsError()
    {
        // Arrange
        var request = new CreateUserRequest("Jane", "Doe", "new@example.com", "P@ssword1", new List<string> { "SuperAdmin" });
        var users = new List<ApplicationUser>().BuildMock();

        _userManagerMock.Setup(m => m.Users).Returns(users);
        _roleServiceMock.Setup(r => r.GetAllAsync(It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleResponse> { new("1", "Member", false) });

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.InvalidCredentials.Code, result.Error.Code);
    }

    [Fact]
    public async Task AddAsync_WhenCreateFails_ReturnsBadRequestError()
    {
        // Arrange
        var request = new CreateUserRequest("Jane", "Doe", "new@example.com", "P@ssword1", new List<string> { "Member" });
        var users = new List<ApplicationUser>().BuildMock();

        _userManagerMock.Setup(m => m.Users).Returns(users);
        _roleServiceMock.Setup(r => r.GetAllAsync(It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleResponse> { new("1", "Member", false) });
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Failed", Description = "Create failed" }));

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Failed", result.Error.Code);
    }

    [Fact]
    public async Task AddAsync_WhenValidRequest_CreatesUserAndAssignsRoles()
    {
        // Arrange
        var request = new CreateUserRequest("Jane", "Doe", "new@example.com", "P@ssword1", new List<string> { "Member" });
        var users = new List<ApplicationUser>().BuildMock();

        _userManagerMock.Setup(m => m.Users).Returns(users);
        _roleServiceMock.Setup(r => r.GetAllAsync(It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleResponse> { new("1", "Member", false) });
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.AddToRolesAsync(It.IsAny<ApplicationUser>(), request.Roles))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("new@example.com", result.Value.Email);
    }

    [Fact]
    public async Task UpdateAsync_WhenDuplicateEmailExists_ReturnsDuplicateEmailError()
    {
        // Arrange
        var request = new UpdateUserRequest("Jane", "Doe", "duplicate@example.com", new List<string> { "Member" });
        var users = new List<ApplicationUser>
        {
            new() { Id = "other-user", Email = "duplicate@example.com" }
        }.BuildMock();

        _userManagerMock.Setup(m => m.Users).Returns(users);

        // Act
        var result = await _sut.UpdateAsync("current-user", request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.DuplicateEmail.Code, result.Error.Code);
    }

    [Fact]
    public async Task UpdateAsync_WhenUserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var request = new UpdateUserRequest("Jane", "Doe", "new@example.com", new List<string> { "Member" });
        var users = new List<ApplicationUser>().BuildMock();

        _userManagerMock.Setup(m => m.Users).Returns(users);
        _roleServiceMock.Setup(r => r.GetAllAsync(It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleResponse> { new("1", "Member", false) });
        _userManagerMock.Setup(m => m.FindByIdAsync("u1"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.UpdateAsync("u1", request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task UpdateAsync_WhenValidRequest_UpdatesUserAndRoles()
    {
        // Arrange
        var request = new UpdateUserRequest("Jane", "Doe", "new@example.com", new List<string> { "Member" });
        var users = new List<ApplicationUser>().BuildMock();
        var user = new ApplicationUser { Id = "u1", Email = "old@example.com" };

        _userManagerMock.Setup(m => m.Users).Returns(users);
        _roleServiceMock.Setup(r => r.GetAllAsync(It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RoleResponse> { new("1", "Member", false) });
        _userManagerMock.Setup(m => m.FindByIdAsync("u1"))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.AddToRolesAsync(user, request.Roles))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.UpdateAsync("u1", request);

        // Assert
        Assert.True(result.IsSuccess);
        _unitOfWorkMock.Verify(u => u.Users.RemoveUserRolesAsync("u1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ToggleStatusAsync_WhenUserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _userManagerMock.Setup(m => m.FindByIdAsync("missing"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.ToggleStatusAsync("missing");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task ToggleStatusAsync_WhenUserExists_TogglesStatusAndReturnsSuccess()
    {
        // Arrange
        var user = new ApplicationUser { Id = "u1", IsDisabled = false };
        _userManagerMock.Setup(m => m.FindByIdAsync("u1"))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.ToggleStatusAsync("u1");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(user.IsDisabled);
    }

    [Fact]
    public async Task UnlockAsync_WhenUserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _userManagerMock.Setup(m => m.FindByIdAsync("missing"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.UnlockAsync("missing");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task UnlockAsync_WhenUserExists_ClearsLockoutEndDateAndReturnsSuccess()
    {
        // Arrange
        var user = new ApplicationUser { Id = "u1" };
        _userManagerMock.Setup(m => m.FindByIdAsync("u1"))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.SetLockoutEndDateAsync(user, null))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.UnlockAsync("u1");

        // Assert
        Assert.True(result.IsSuccess);
        _userManagerMock.Verify(m => m.SetLockoutEndDateAsync(user, null), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenSucceeded_ReturnsSuccess()
    {
        // Arrange
        var user = new ApplicationUser { Id = "u1" };
        var request = new ChangePasswordRequest("OldP@ss1", "NewP@ss2");

        _userManagerMock.Setup(m => m.FindByIdAsync("u1"))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.ChangePasswordAsync("u1", request);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenFailed_ReturnsBadRequestError()
    {
        // Arrange
        var user = new ApplicationUser { Id = "u1" };
        var request = new ChangePasswordRequest("WrongOld", "NewP@ss2");

        _userManagerMock.Setup(m => m.FindByIdAsync("u1"))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "PasswordMismatch", Description = "Incorrect password" }));

        // Act
        var result = await _sut.ChangePasswordAsync("u1", request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("PasswordMismatch", result.Error.Code);
    }
}
