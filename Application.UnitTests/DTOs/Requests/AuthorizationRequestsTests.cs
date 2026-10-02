using Application.DTOs.Requests.Authorization;
using Xunit;

namespace Application.UnitTests.DTOs.Requests;

public class AuthorizationRequestsTests
{
    [Fact]
    public void ConfirmEmailRequest_WhenInstantiated_HoldsValues()
    {
        // Act
        var request = new ConfirmEmailRequest("user-1", "code-123");

        // Assert
        Assert.Equal("user-1", request.UserId);
        Assert.Equal("code-123", request.Code);
    }

    [Fact]
    public void ForgetPasswordRequest_WhenInstantiated_HoldsValues()
    {
        // Act
        var request = new ForgetPasswordRequest("user@example.com");

        // Assert
        Assert.Equal("user@example.com", request.Email);
    }

    [Fact]
    public void LoginRequest_WhenInstantiated_HoldsValues()
    {
        // Act
        var request = new LoginRequest("user@example.com", "Password123!");

        // Assert
        Assert.Equal("user@example.com", request.Email);
        Assert.Equal("Password123!", request.Password);
    }

    [Fact]
    public void RefreshTokenRequest_WhenInstantiated_HoldsValues()
    {
        // Act
        var request = new RefreshTokenRequest("jwt-token", "refresh-token");

        // Assert
        Assert.Equal("jwt-token", request.Token);
        Assert.Equal("refresh-token", request.RefreshToken);
    }

    [Fact]
    public void RegisterRequest_WhenInstantiated_HoldsValues()
    {
        // Act
        var request = new RegisterRequest("user@example.com", "John", "Doe", "Password123!");

        // Assert
        Assert.Equal("user@example.com", request.Email);
        Assert.Equal("John", request.FirstName);
        Assert.Equal("Doe", request.LastName);
        Assert.Equal("Password123!", request.Password);
    }

    [Fact]
    public void ResendConfirmationEmailRequest_WhenInstantiated_HoldsValues()
    {
        // Act
        var request = new ResendConfirmationEmailRequest("user@example.com");

        // Assert
        Assert.Equal("user@example.com", request.Email);
    }

    [Fact]
    public void ResetPasswordRequest_WhenInstantiated_HoldsValues()
    {
        // Act
        var request = new ResetPasswordRequest("user@example.com", "code-123", "NewPassword123!");

        // Assert
        Assert.Equal("user@example.com", request.Email);
        Assert.Equal("code-123", request.Code);
        Assert.Equal("NewPassword123!", request.NewPassword);
    }
}
