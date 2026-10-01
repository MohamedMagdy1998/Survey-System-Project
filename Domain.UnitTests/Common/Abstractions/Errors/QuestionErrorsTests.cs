using Domain.Common.Abstractions.Errors;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Domain.UnitTests.Common.Abstractions.Errors;

public class QuestionErrorsTests
{
    [Fact]
    public void NotFound_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = QuestionErrors.NotFound;

        // Assert
        Assert.Equal("Question.NotFound", error.Code);
        Assert.Equal("The requested question was not found.", error.Description);
        Assert.Equal(StatusCodes.Status404NotFound, error.StatusCode);
    }

    [Fact]
    public void DuplicateContent_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = QuestionErrors.DuplicateContent;

        // Assert
        Assert.Equal("Question.DuplicateContent", error.Code);
        Assert.Equal("A question with the same content already exists.", error.Description);
        Assert.Equal(StatusCodes.Status409Conflict, error.StatusCode);
    }
}
