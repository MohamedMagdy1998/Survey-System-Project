using Domain.Common.Abstractions.Errors;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Domain.UnitTests.Common.Abstractions.Errors;

public class VoteErrorsTests
{
    [Fact]
    public void NotFound_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = VoteErrors.NotFound;

        // Assert
        Assert.Equal("Vote.NotFound", error.Code);
        Assert.Equal("The requested vote was not found.", error.Description);
        Assert.Equal(StatusCodes.Status404NotFound, error.StatusCode);
    }

    [Fact]
    public void DuplicateContent_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = VoteErrors.DuplicateContent;

        // Assert
        Assert.Equal("Vote.DuplicateContent", error.Code);
        Assert.Equal("A vote with the same content already exists.", error.Description);
        Assert.Equal(StatusCodes.Status409Conflict, error.StatusCode);
    }
}
