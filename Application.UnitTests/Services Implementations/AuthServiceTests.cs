using Application.DTOs.Requests.Authorization;
using Application.Services_Implementations;
using Domain.Common.Abstractions.Errors;
using Domain.Common.Const;
using Domain.Common.Interfaces;
using Domain.Contracts;
using Domain.Models;
using Hangfire;
using Hangfire.Common;
using Hangfire.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Application.UnitTests.Common.TestHelpers;
using Moq;
using System.Text;
using Xunit;

namespace Application.UnitTests.Services_Implementations;

public class AuthServiceTests
{
    private readonly Mock<IJwtProvider> _jwtProviderMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock = new();
    private readonly Mock<IEmailSender> _emailSenderMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        InitializeHangfireStorage();

        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Origin"] = "https://example.com";
        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(httpContext);

        _sut = new AuthService(
            _jwtProviderMock.Object,
            _userManagerMock.Object,
            _loggerMock.Object,
            _emailSenderMock.Object,
            _httpContextAccessorMock.Object,
            _unitOfWorkMock.Object);
    }

    private static void InitializeHangfireStorage()
    {
        var storage = new Mock<JobStorage>();
        var connection = new Mock<IStorageConnection>();
        var transaction = new Mock<IWriteOnlyTransaction>();
        connection.Setup(c => c.CreateWriteTransaction()).Returns(transaction.Object);
        connection.Setup(c => c.CreateExpiredJob(It.IsAny<Job>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>()))
            .Returns("job-id");
        storage.Setup(s => s.GetConnection()).Returns(connection.Object);
        JobStorage.Current = storage.Object;
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ReturnsDuplicateEmailError()
    {
        // Arrange
        var request = new RegisterRequest("existing@example.com", "John", "Doe", "P@ssword123!");
        var users = new List<ApplicationUser> { new() { Email = "existing@example.com" } }.BuildMock();

        _userManagerMock.Setup(m => m.Users).Returns(users);

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.DuplicateEmail.Code, result.Error.Code);
    }

    [Fact]
    public async Task RegisterAsync_WhenCreateUserFails_ReturnsBadRequestError()
    {
        // Arrange
        var request = new RegisterRequest("new@example.com", "John", "Doe", "P@ssword123!");
        var users = new List<ApplicationUser>().BuildMock();

        _userManagerMock.Setup(m => m.Users).Returns(users);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "Error", Description = "Create failed" }));

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Error", result.Error.Code);
    }

    [Fact]
    public async Task RegisterAsync_WhenValidRequest_CreatesUserAndReturnsSuccess()
    {
        // Arrange
        var request = new RegisterRequest("new@example.com", "John", "Doe", "P@ssword123!");
        var users = new List<ApplicationUser>().BuildMock();

        _userManagerMock.Setup(m => m.Users).Returns(users);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("token-123");

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ConfirmEmailAsync_WhenUserNotFound_ReturnsInvalidCodeError()
    {
        // Arrange
        var request = new ConfirmEmailRequest("missing", "code");
        _userManagerMock.Setup(m => m.FindByIdAsync("missing"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.ConfirmEmailAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.InvalidCode.Code, result.Error.Code);
    }

    [Fact]
    public async Task ConfirmEmailAsync_WhenAlreadyConfirmed_ReturnsDuplicatedConfirmationError()
    {
        // Arrange
        var user = new ApplicationUser { Id = "u1", EmailConfirmed = true };
        var request = new ConfirmEmailRequest("u1", "code");
        _userManagerMock.Setup(m => m.FindByIdAsync("u1"))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.ConfirmEmailAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.DuplicatedConfirmation.Code, result.Error.Code);
    }

    [Fact]
    public async Task ConfirmEmailAsync_WhenCodeFormatIsInvalid_ReturnsInvalidCodeError()
    {
        // Arrange
        var user = new ApplicationUser { Id = "u1", EmailConfirmed = false };
        var request = new ConfirmEmailRequest("u1", "%%%invalid-base64%%%");
        _userManagerMock.Setup(m => m.FindByIdAsync("u1"))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.ConfirmEmailAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.InvalidCode.Code, result.Error.Code);
    }

    [Fact]
    public async Task ConfirmEmailAsync_WhenConfirmationSucceeds_AddsRoleAndReturnsSuccess()
    {
        // Arrange
        var user = new ApplicationUser { Id = "u1", EmailConfirmed = false };
        var validCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("raw-token"));
        var request = new ConfirmEmailRequest("u1", validCode);

        _userManagerMock.Setup(m => m.FindByIdAsync("u1"))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.ConfirmEmailAsync(user, "raw-token"))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.AddToRoleAsync(user, DefaultRoles.Member))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.ConfirmEmailAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        _userManagerMock.Verify(m => m.AddToRoleAsync(user, DefaultRoles.Member), Times.Once);
    }

    [Fact]
    public async Task ResendConfirmationEmailAsync_WhenUserNotFound_ReturnsSuccess()
    {
        // Arrange
        var request = new ResendConfirmationEmailRequest("missing@example.com");
        _userManagerMock.Setup(m => m.FindByEmailAsync("missing@example.com"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.ResendConfirmationEmailAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ResendConfirmationEmailAsync_WhenAlreadyConfirmed_ReturnsDuplicatedConfirmationError()
    {
        // Arrange
        var user = new ApplicationUser { Id = "u1", EmailConfirmed = true };
        var request = new ResendConfirmationEmailRequest("confirmed@example.com");
        _userManagerMock.Setup(m => m.FindByEmailAsync("confirmed@example.com"))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.ResendConfirmationEmailAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.DuplicatedConfirmation.Code, result.Error.Code);
    }

    [Fact]
    public async Task ResendConfirmationEmailAsync_WhenNotConfirmed_SendsEmailAndReturnsSuccess()
    {
        // Arrange
        var user = new ApplicationUser { Id = "u1", Email = "user@example.com", EmailConfirmed = false };
        var request = new ResendConfirmationEmailRequest("user@example.com");

        _userManagerMock.Setup(m => m.FindByEmailAsync("user@example.com"))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync("token-123");

        // Act
        var result = await _sut.ResendConfirmationEmailAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task GetTokenAsync_WhenUserNotFound_ReturnsInvalidCredentialsError()
    {
        // Arrange
        _userManagerMock.Setup(m => m.FindByEmailAsync("missing@example.com"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.GetTokenAsync("missing@example.com", "Password", CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.InvalidCredentials.Code, result.Error.Code);
    }

    [Fact]
    public async Task GetTokenAsync_WhenPasswordIsIncorrect_ReturnsInvalidCredentialsError()
    {
        // Arrange
        var user = new ApplicationUser { Id = "u1", Email = "user@example.com" };
        _userManagerMock.Setup(m => m.FindByEmailAsync("user@example.com"))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "WrongPass"))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.GetTokenAsync("user@example.com", "WrongPass", CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.InvalidCredentials.Code, result.Error.Code);
    }

    [Fact]
    public async Task GetTokenAsync_WhenCredentialsAreValid_ReturnsAuthResponseWithToken()
    {
        // Arrange
        var user = new ApplicationUser { Id = "u1", Email = "user@example.com", FirstName = "John", LastName = "Doe" };
        _userManagerMock.Setup(m => m.FindByEmailAsync("user@example.com"))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, "ValidPass1!"))
            .ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.Roles.GetUserRolesAndPermissions(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<string> { "Member" }, new List<string> { Permissions.GetPolls }));
        _jwtProviderMock.Setup(j => j.GenerateToken(user, It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
            .Returns(("new-jwt-token", 3600));

        // Act
        var result = await _sut.GetTokenAsync("user@example.com", "ValidPass1!", CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("new-jwt-token", result.Value.Token);
        Assert.NotEmpty(user.RefreshTokens);
    }

    [Fact]
    public async Task GetNewTokenAndRefreshTokenAsync_WhenTokenIsInvalid_ReturnsInvalidCredentialsError()
    {
        // Arrange
        _jwtProviderMock.Setup(j => j.ValidateToken("invalid-token"))
            .Returns((string?)null);

        // Act
        var result = await _sut.GetNewTokenAndRefreshTokenAsync("invalid-token", "refresh-token");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.InvalidCredentials.Code, result.Error.Code);
    }

    [Fact]
    public async Task GetNewTokenAndRefreshTokenAsync_WhenUserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _jwtProviderMock.Setup(j => j.ValidateToken("valid-token"))
            .Returns("u1");
        _userManagerMock.Setup(m => m.FindByIdAsync("u1"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.GetNewTokenAndRefreshTokenAsync("valid-token", "refresh-token");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task GetNewTokenAndRefreshTokenAsync_WhenRefreshTokenIsNotActive_ReturnsInvalidCredentialsError()
    {
        // Arrange
        var user = new ApplicationUser { Id = "u1" };
        user.RefreshTokens.Add(new RefreshToken { Token = "expired-token", ExpiresOn = DateTime.UtcNow.AddHours(-1) });

        _jwtProviderMock.Setup(j => j.ValidateToken("valid-jwt"))
            .Returns("u1");
        _userManagerMock.Setup(m => m.FindByIdAsync("u1"))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetNewTokenAndRefreshTokenAsync("valid-jwt", "expired-token");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.InvalidCredentials.Code, result.Error.Code);
    }

    [Fact]
    public async Task GetNewTokenAndRefreshTokenAsync_WhenValid_ReturnsNewTokens()
    {
        // Arrange
        var user = new ApplicationUser { Id = "u1", Email = "user@example.com", FirstName = "John", LastName = "Doe" };
        var activeRefreshToken = new RefreshToken { Token = "valid-refresh", ExpiresOn = DateTime.UtcNow.AddDays(5) };
        user.RefreshTokens.Add(activeRefreshToken);

        _jwtProviderMock.Setup(j => j.ValidateToken("valid-jwt"))
            .Returns("u1");
        _userManagerMock.Setup(m => m.FindByIdAsync("u1"))
            .ReturnsAsync(user);
        _unitOfWorkMock.Setup(u => u.Roles.GetUserRolesAndPermissions(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<string>(), new List<string>()));
        _jwtProviderMock.Setup(j => j.GenerateToken(user, It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
            .Returns(("brand-new-jwt", 3600));

        // Act
        var result = await _sut.GetNewTokenAndRefreshTokenAsync("valid-jwt", "valid-refresh");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(activeRefreshToken.RevokedOn);
        Assert.Equal("brand-new-jwt", result.Value.Token);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_WhenTokenInvalid_ReturnsNotFoundError()
    {
        // Arrange
        _jwtProviderMock.Setup(j => j.ValidateToken("invalid-jwt"))
            .Returns((string?)null);

        // Act
        var result = await _sut.RevokeRefreshTokenAsync("invalid-jwt", "any-refresh");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_WhenUserNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _jwtProviderMock.Setup(j => j.ValidateToken("valid-jwt"))
            .Returns("u1");
        _userManagerMock.Setup(m => m.FindByIdAsync("u1"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.RevokeRefreshTokenAsync("valid-jwt", "any-refresh");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_WhenRefreshTokenNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var user = new ApplicationUser { Id = "u1" };
        _jwtProviderMock.Setup(j => j.ValidateToken("valid-jwt"))
            .Returns("u1");
        _userManagerMock.Setup(m => m.FindByIdAsync("u1"))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.RevokeRefreshTokenAsync("valid-jwt", "missing-refresh");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_WhenValid_RevokesTokenAndReturnsSuccess()
    {
        // Arrange
        var user = new ApplicationUser { Id = "u1" };
        var rt = new RefreshToken { Token = "target-refresh", ExpiresOn = DateTime.UtcNow.AddDays(5) };
        user.RefreshTokens.Add(rt);

        _jwtProviderMock.Setup(j => j.ValidateToken("valid-jwt"))
            .Returns("u1");
        _userManagerMock.Setup(m => m.FindByIdAsync("u1"))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.RevokeRefreshTokenAsync("valid-jwt", "target-refresh");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(rt.RevokedOn);
        _userManagerMock.Verify(m => m.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task SendResetPasswordCodeAsync_WhenUserNotFound_ReturnsSuccess()
    {
        // Arrange
        _userManagerMock.Setup(m => m.FindByEmailAsync("missing@example.com"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.SendResetPasswordCodeAsync("missing@example.com");

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task SendResetPasswordCodeAsync_WhenEmailNotConfirmed_ReturnsEmailNotConfirmedError()
    {
        // Arrange
        var user = new ApplicationUser { Id = "u1", EmailConfirmed = false };
        _userManagerMock.Setup(m => m.FindByEmailAsync("unconfirmed@example.com"))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.SendResetPasswordCodeAsync("unconfirmed@example.com");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.EmailNotConfirmed.Code, result.Error.Code);
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenUserNotFoundOrUnconfirmed_ReturnsInvalidCodeError()
    {
        // Arrange
        var request = new ResetPasswordRequest("missing@example.com", "code", "NewPass1!");
        _userManagerMock.Setup(m => m.FindByEmailAsync("missing@example.com"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _sut.ResetPasswordAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserErrors.InvalidCode.Code, result.Error.Code);
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenTokenDecodesAndResetSucceeds_ReturnsSuccess()
    {
        // Arrange
        var user = new ApplicationUser { Id = "u1", EmailConfirmed = true };
        var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("reset-token"));
        var request = new ResetPasswordRequest("user@example.com", encodedCode, "NewPass1!");

        _userManagerMock.Setup(m => m.FindByEmailAsync("user@example.com"))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.ResetPasswordAsync(user, "reset-token", "NewPass1!"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _sut.ResetPasswordAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
    }
}
