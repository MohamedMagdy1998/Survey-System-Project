using Domain.Common.Abstractions.Errors;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Domain.UnitTests.Common.Abstractions.Errors;

public class PollErrorsTests
{
    [Fact]
    public void NotFound_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = PollErrors.NotFound;

        // Assert
        Assert.Equal("Polls.NotFound", error.Code);
        Assert.Equal("The requested poll was not found.", error.Description);
        Assert.Equal(StatusCodes.Status404NotFound, error.StatusCode);
    }

    [Fact]
    public void DuplicateTitle_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = PollErrors.DuplicateTitle;

        // Assert
        Assert.Equal("Polls.DuplicateTitle", error.Code);
        Assert.Equal("A poll with the same title already exists.", error.Description);
        Assert.Equal(StatusCodes.Status409Conflict, error.StatusCode);
    }

    [Fact]
    public void InvalidDates_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = PollErrors.InvalidDates;

        // Assert
        Assert.Equal("Polls.InvalidDates", error.Code);
        Assert.Equal("The end date must be after the start date.", error.Description);
        Assert.Equal(StatusCodes.Status400BadRequest, error.StatusCode);
    }

    [Fact]
    public void PastStartDate_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = PollErrors.PastStartDate;

        // Assert
        Assert.Equal("Polls.PastStartDate", error.Code);
        Assert.Equal("The start date cannot be set in the past.", error.Description);
        Assert.Equal(StatusCodes.Status400BadRequest, error.StatusCode);
    }

    [Fact]
    public void AlreadyPublished_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = PollErrors.AlreadyPublished;

        // Assert
        Assert.Equal("Polls.AlreadyPublished", error.Code);
        Assert.Equal("Published polls cannot be modified.", error.Description);
        Assert.Equal(StatusCodes.Status409Conflict, error.StatusCode);
    }

    [Fact]
    public void NotPublished_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = PollErrors.NotPublished;

        // Assert
        Assert.Equal("Polls.NotPublished", error.Code);
        Assert.Equal("This poll is not published yet.", error.Description);
        Assert.Equal(StatusCodes.Status400BadRequest, error.StatusCode);
    }

    [Fact]
    public void VotingClosed_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = PollErrors.VotingClosed;

        // Assert
        Assert.Equal("Polls.VotingClosed", error.Code);
        Assert.Equal("The voting period for this poll has ended.", error.Description);
        Assert.Equal(StatusCodes.Status400BadRequest, error.StatusCode);
    }

    [Fact]
    public void AlreadyVoted_WhenAccessed_ReturnsExpectedError()
    {
        // Act
        var error = PollErrors.AlreadyVoted;

        // Assert
        Assert.Equal("Polls.AlreadyVoted", error.Code);
        Assert.Equal("You have already submitted a vote for this poll.", error.Description);
        Assert.Equal(StatusCodes.Status409Conflict, error.StatusCode);
    }
}
