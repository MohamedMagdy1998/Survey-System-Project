using Domain.Common.Abstractions.Errors;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Domain.UnitTests.Common.Abstractions.Errors;

public class ErrorTests
{
    [Fact]
    public void None_WhenAccessed_ReturnsEmptyErrorWithNullStatusCode()
    {
        // Act
        var error = Error.None;

        // Assert
        Assert.Equal(string.Empty, error.Code);
        Assert.Equal(string.Empty, error.Description);
        Assert.Null(error.StatusCode);
    }

    [Fact]
    public void BadRequest_WhenCalled_ReturnsErrorWithStatus400BadRequest()
    {
        // Arrange
        const string code = "Bad.Request";
        const string description = "Invalid request payload";

        // Act
        var error = Error.BadRequest(code, description);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(description, error.Description);
        Assert.Equal(StatusCodes.Status400BadRequest, error.StatusCode);
    }

    [Fact]
    public void Unauthorized_WhenCalled_ReturnsErrorWithStatus401Unauthorized()
    {
        // Arrange
        const string code = "Auth.Unauthorized";
        const string description = "Authentication required";

        // Act
        var error = Error.Unauthorized(code, description);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(description, error.Description);
        Assert.Equal(StatusCodes.Status401Unauthorized, error.StatusCode);
    }

    [Fact]
    public void Forbidden_WhenCalled_ReturnsErrorWithStatus403Forbidden()
    {
        // Arrange
        const string code = "Auth.Forbidden";
        const string description = "Access denied";

        // Act
        var error = Error.Forbidden(code, description);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(description, error.Description);
        Assert.Equal(StatusCodes.Status403Forbidden, error.StatusCode);
    }

    [Fact]
    public void NotFound_WhenCalled_ReturnsErrorWithStatus404NotFound()
    {
        // Arrange
        const string code = "Resource.NotFound";
        const string description = "Resource was not found";

        // Act
        var error = Error.NotFound(code, description);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(description, error.Description);
        Assert.Equal(StatusCodes.Status404NotFound, error.StatusCode);
    }

    [Fact]
    public void Conflict_WhenCalled_ReturnsErrorWithStatus409Conflict()
    {
        // Arrange
        const string code = "Resource.Conflict";
        const string description = "Conflict with current state";

        // Act
        var error = Error.Conflict(code, description);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(description, error.Description);
        Assert.Equal(StatusCodes.Status409Conflict, error.StatusCode);
    }

    [Fact]
    public void Custom_WhenCalled_ReturnsErrorWithSpecifiedStatusCode()
    {
        // Arrange
        const string code = "Custom.Error";
        const string description = "Custom error message";
        const int statusCode = StatusCodes.Status422UnprocessableEntity;

        // Act
        var error = Error.Custom(code, description, statusCode);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(description, error.Description);
        Assert.Equal(statusCode, error.StatusCode);
    }

    [Fact]
    public void Equals_WhenErrorsHaveSameValues_ReturnsTrue()
    {
        // Arrange
        var error1 = Error.BadRequest("Same.Code", "Same Description");
        var error2 = Error.BadRequest("Same.Code", "Same Description");

        // Act & Assert
        Assert.Equal(error1, error2);
        Assert.True(error1 == error2);
    }

    [Fact]
    public void Equals_WhenErrorsHaveDifferentValues_ReturnsFalse()
    {
        // Arrange
        var error1 = Error.BadRequest("Code.A", "Description A");
        var error2 = Error.BadRequest("Code.B", "Description B");

        // Act & Assert
        Assert.NotEqual(error1, error2);
        Assert.True(error1 != error2);
    }
}
