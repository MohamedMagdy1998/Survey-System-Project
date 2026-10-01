using Domain.Common.Abstractions.Errors;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Domain.UnitTests.Common.Abstractions.Errors;

public class UserErrorsTests
{
    [Fact]
    public void NotFound_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = UserErrors.NotFound;

        // Assert
        Assert.Equal("Users.NotFound", error.Code);
        Assert.Equal("The requested user was not found.", error.Description);
        Assert.Equal(StatusCodes.Status404NotFound, error.StatusCode);
    }

    [Fact]
    public void InvalidCredentials_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = UserErrors.InvalidCredentials;

        // Assert
        Assert.Equal("Users.InvalidCredentials", error.Code);
        Assert.Equal("Invalid email or password.", error.Description);
        Assert.Equal(StatusCodes.Status400BadRequest, error.StatusCode);
    }

    [Fact]
    public void InvalidCode_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = UserErrors.InvalidCode;

        // Assert
        Assert.Equal("Users.InvalidCode", error.Code);
        Assert.Equal("Invalid Code.", error.Description);
        Assert.Equal(StatusCodes.Status400BadRequest, error.StatusCode);
    }

    [Fact]
    public void DuplicateEmail_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = UserErrors.DuplicateEmail;

        // Assert
        Assert.Equal("Users.DuplicateEmail", error.Code);
        Assert.Equal("A user with this email address already exists.", error.Description);
        Assert.Equal(StatusCodes.Status409Conflict, error.StatusCode);
    }

    [Fact]
    public void Unauthorized_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = UserErrors.Unauthorized;

        // Assert
        Assert.Equal("Users.Unauthorized", error.Code);
        Assert.Equal("You must be logged in to perform this action.", error.Description);
        Assert.Equal(StatusCodes.Status401Unauthorized, error.StatusCode);
    }

    [Fact]
    public void Forbidden_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = UserErrors.Forbidden;

        // Assert
        Assert.Equal("Users.Forbidden", error.Code);
        Assert.Equal("You do not have permission to access this resource.", error.Description);
        Assert.Equal(StatusCodes.Status403Forbidden, error.StatusCode);
    }

    [Fact]
    public void AccountLocked_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = UserErrors.AccountLocked;

        // Assert
        Assert.Equal("Users.AccountLocked", error.Code);
        Assert.Equal("This account has been locked due to multiple failed login attempts.", error.Description);
        Assert.Equal(StatusCodes.Status400BadRequest, error.StatusCode);
    }

    [Fact]
    public void EmailNotConfirmed_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = UserErrors.EmailNotConfirmed;

        // Assert
        Assert.Equal("Users.EmailNotConfirmed", error.Code);
        Assert.Equal("Email address has not been confirmed yet.", error.Description);
        Assert.Equal(StatusCodes.Status400BadRequest, error.StatusCode);
    }

    [Fact]
    public void DuplicatedConfirmation_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = UserErrors.DuplicatedConfirmation;

        // Assert
        Assert.Equal("Users.DuplicatedConfirmation", error.Code);
        Assert.Equal("A user Confirmed Already.", error.Description);
        Assert.Equal(StatusCodes.Status409Conflict, error.StatusCode);
    }
}
