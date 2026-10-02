using Application.DTOs.Requests.Votes;
using Application.Services_Implementations;
using Domain.Common.Abstractions.Errors;
using Domain.Contracts;
using Domain.Models;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace Application.UnitTests.Services_Implementations;

public class VoteServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly VoteService _sut;

    public VoteServiceTests()
    {
        _sut = new VoteService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task AddAsync_WhenUserAlreadyVoted_ReturnsDuplicateContentError()
    {
        // Arrange
        var pollId = 1;
        var userId = "user-1";
        var request = new VoteRequest(new List<VoteAnswerRequest> { new(1, 10) });

        _unitOfWorkMock.Setup(u => u.Votes.HasUserVotedAsync(pollId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.AddAsync(pollId, userId, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(VoteErrors.DuplicateContent.Code, result.Error.Code);
    }

    [Fact]
    public async Task AddAsync_WhenPollDoesNotExistOrNotActive_ReturnsPollNotFoundError()
    {
        // Arrange
        var pollId = 1;
        var userId = "user-1";
        var request = new VoteRequest(new List<VoteAnswerRequest> { new(1, 10) });

        _unitOfWorkMock.Setup(u => u.Votes.HasUserVotedAsync(pollId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _unitOfWorkMock.Setup(u => u.Polls.ExistsAsync(It.IsAny<Expression<Func<Poll, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.AddAsync(pollId, userId, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task AddAsync_WhenSubmittedQuestionsDoNotMatchActiveQuestions_ReturnsVoteNotFoundError()
    {
        // Arrange
        var pollId = 1;
        var userId = "user-1";
        var request = new VoteRequest(new List<VoteAnswerRequest> { new(1, 10) });

        _unitOfWorkMock.Setup(u => u.Votes.HasUserVotedAsync(pollId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _unitOfWorkMock.Setup(u => u.Polls.ExistsAsync(It.IsAny<Expression<Func<Poll, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.Questions.GetActiveQuestionIdsAsync(pollId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 1, 2 }); // Active questions are 1 and 2, but only 1 was submitted

        // Act
        var result = await _sut.AddAsync(pollId, userId, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(VoteErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task AddAsync_WhenVoteIsValid_AddsVoteAndReturnsSuccess()
    {
        // Arrange
        var pollId = 1;
        var userId = "user-1";
        var request = new VoteRequest(new List<VoteAnswerRequest> { new(1, 10) });

        _unitOfWorkMock.Setup(u => u.Votes.HasUserVotedAsync(pollId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _unitOfWorkMock.Setup(u => u.Polls.ExistsAsync(It.IsAny<Expression<Func<Poll, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.Questions.GetActiveQuestionIdsAsync(pollId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<int> { 1 });

        // Act
        var result = await _sut.AddAsync(pollId, userId, request);

        // Assert
        Assert.True(result.IsSuccess);
        _unitOfWorkMock.Verify(u => u.Votes.AddAsync(It.Is<Vote>(v => v.PollId == pollId && v.UserId == userId), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
