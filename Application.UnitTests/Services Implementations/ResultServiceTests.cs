using Application.DTOs.Responses.Result;
using Application.Services_Implementations;
using Domain.Common.Abstractions.Errors;
using Domain.Contracts;
using Domain.Models;
using Domain.Models.Result;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace Application.UnitTests.Services_Implementations;

public class ResultServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ResultService _sut;

    public ResultServiceTests()
    {
        _sut = new ResultService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetPollVotesAsync_WhenPollVotesFound_ReturnsSuccessWithPollVotes()
    {
        // Arrange
        var pollId = 1;
        var expectedResponse = new PollVotesResponse("Poll 1", new List<VoteResponse>());

        _unitOfWorkMock.Setup(u => u.Votes.GetPollVotesAsync(pollId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _sut.GetPollVotesAsync(pollId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedResponse.Title, result.Value.Title);
    }

    [Fact]
    public async Task GetPollVotesAsync_WhenPollVotesNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var pollId = 99;

        _unitOfWorkMock.Setup(u => u.Votes.GetPollVotesAsync(pollId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PollVotesResponse?)null);

        // Act
        var result = await _sut.GetPollVotesAsync(pollId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task GetVotesPerDayAsync_WhenPollDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        var pollId = 1;

        _unitOfWorkMock.Setup(u => u.Polls.ExistsAsync(It.IsAny<Expression<Func<Poll, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.GetVotesPerDayAsync(pollId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task GetVotesPerDayAsync_WhenPollExists_ReturnsSuccessWithVotesPerDay()
    {
        // Arrange
        var pollId = 1;
        var expectedVotes = new List<VotesPerDayResponse>
        {
            new(DateOnly.FromDateTime(DateTime.Today), 5)
        };

        _unitOfWorkMock.Setup(u => u.Polls.ExistsAsync(It.IsAny<Expression<Func<Poll, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.Votes.GetVotesPerDayAsync(pollId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVotes);

        // Act
        var result = await _sut.GetVotesPerDayAsync(pollId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedVotes, result.Value);
    }

    [Fact]
    public async Task GetVotesPerQuestionAsync_WhenPollDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        var pollId = 1;

        _unitOfWorkMock.Setup(u => u.Polls.ExistsAsync(It.IsAny<Expression<Func<Poll, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.GetVotesPerQuestionAsync(pollId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task GetVotesPerQuestionAsync_WhenPollExists_ReturnsSuccessWithVotesPerQuestion()
    {
        // Arrange
        var pollId = 1;
        var expectedVotes = new List<VotesPerQuestionResponse>
        {
            new("Question 1", new List<VotesPerAnswerResponse>())
        };

        _unitOfWorkMock.Setup(u => u.Polls.ExistsAsync(It.IsAny<Expression<Func<Poll, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.Votes.GetVotesPerQuestionAsync(pollId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedVotes);

        // Act
        var result = await _sut.GetVotesPerQuestionAsync(pollId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedVotes, result.Value);
    }
}
