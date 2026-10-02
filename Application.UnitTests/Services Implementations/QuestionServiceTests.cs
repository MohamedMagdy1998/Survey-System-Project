using Application.Common.Contracts;
using Application.DTOs.Requests.Questions;
using Application.DTOs.Responses.Answers;
using Application.DTOs.Responses.Questions;
using Application.Services_Implementations;
using Application.Services_Interfaces;
using Domain.Common.Abstractions.Errors;
using Domain.Contracts;
using Domain.Models;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace Application.UnitTests.Services_Implementations;

public class QuestionServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();
    private readonly QuestionService _sut;

    public QuestionServiceTests()
    {
        new global::Application.Mapping.MappingConfigurations().Register(Mapster.TypeAdapterConfig.GlobalSettings);
        _sut = new QuestionService(_unitOfWorkMock.Object, _cacheServiceMock.Object);
    }

    [Fact]
    public async Task AddAsync_WhenPollDoesNotExist_ReturnsPollNotFoundError()
    {
        // Arrange
        var request = new QuestionRequest("Question content", new List<string> { "A1", "A2" });
        _unitOfWorkMock.Setup(u => u.Polls.ExistsAsync(It.IsAny<Expression<Func<Poll, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.AddAsync(1, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task AddAsync_WhenQuestionContentAlreadyExists_ReturnsDuplicateContentError()
    {
        // Arrange
        var request = new QuestionRequest("Duplicate question content", new List<string> { "A1", "A2" });
        _unitOfWorkMock.Setup(u => u.Polls.ExistsAsync(It.IsAny<Expression<Func<Poll, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.Questions.ExistsAsync(It.IsAny<Expression<Func<Question, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.AddAsync(1, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(QuestionErrors.DuplicateContent.Code, result.Error.Code);
    }

    [Fact]
    public async Task AddAsync_WhenValidRequest_AddsQuestionAndClearsCache()
    {
        // Arrange
        var pollId = 1;
        var request = new QuestionRequest("New Question", new List<string> { "A1", "A2" });
        _unitOfWorkMock.Setup(u => u.Polls.ExistsAsync(It.IsAny<Expression<Func<Poll, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.Questions.ExistsAsync(It.IsAny<Expression<Func<Question, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.AddAsync(pollId, request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("New Question", result.Value.Content);
        _unitOfWorkMock.Verify(u => u.Questions.AddAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync($"questions:poll:{pollId}:all", It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync($"questions:poll:{pollId}:available", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WhenCached_ReturnsCachedQuestions()
    {
        // Arrange
        var pollId = 1;
        var cached = new List<QuestionResponse>
        {
            new(1, "Cached Question", new List<AnswerResponse>())
        };

        _cacheServiceMock.Setup(c => c.GetAsync<List<QuestionResponse>>($"questions:poll:{pollId}:all", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        // Act
        var result = await _sut.GetAllAsync(pollId, new RequestFilters());

        // Assert
        Assert.True(result.IsSuccess);
        var page = Assert.Single(result.Value);
        Assert.Single(page.Items);
    }

    [Fact]
    public async Task GetAllAsync_WhenPollNotFound_ReturnsPollNotFoundError()
    {
        // Arrange
        var pollId = 1;
        _cacheServiceMock.Setup(c => c.GetAsync<List<QuestionResponse>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<QuestionResponse>?)null);
        _unitOfWorkMock.Setup(u => u.Polls.ExistsAsync(It.IsAny<Expression<Func<Poll, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.GetAllAsync(pollId, new RequestFilters());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoQuestionsExist_ReturnsQuestionNotFoundError()
    {
        // Arrange
        var pollId = 1;
        _cacheServiceMock.Setup(c => c.GetAsync<List<QuestionResponse>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<QuestionResponse>?)null);
        _unitOfWorkMock.Setup(u => u.Polls.ExistsAsync(It.IsAny<Expression<Func<Poll, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _unitOfWorkMock.Setup(u => u.Questions.GetAllAsync(pollId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Question>());

        // Act
        var result = await _sut.GetAllAsync(pollId, new RequestFilters());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(QuestionErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task GetAvailableAsync_WhenUserAlreadyVoted_ReturnsVoteDuplicateContentError()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.Votes.ExistsAsync(1, "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.GetAvailableAsync(1, "user-1");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(VoteErrors.DuplicateContent.Code, result.Error.Code);
    }

    [Fact]
    public async Task GetAvailableAsync_WhenPollNotFoundOrInactive_ReturnsPollNotFoundError()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.Votes.ExistsAsync(1, "user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _unitOfWorkMock.Setup(u => u.Polls.ExistsAsync(It.IsAny<Expression<Func<Poll, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.GetAvailableAsync(1, "user-1");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task GetByIdAsync_WhenCached_ReturnsCachedQuestion()
    {
        // Arrange
        var cached = new QuestionResponse(1, "Cached", new List<AnswerResponse>());
        _cacheServiceMock.Setup(c => c.GetAsync<QuestionResponse>("questions:poll:1:1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        // Act
        var result = await _sut.GetByIdAsync(1, 1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Cached", result.Value.Content);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFoundInDb_ReturnsQuestionNotFoundError()
    {
        // Arrange
        _cacheServiceMock.Setup(c => c.GetAsync<QuestionResponse>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((QuestionResponse?)null);
        _unitOfWorkMock.Setup(u => u.Questions.GetAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Question?)null);

        // Act
        var result = await _sut.GetByIdAsync(1, 1);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(QuestionErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task UpdateAsync_WhenQuestionNotFound_ReturnsQuestionNotFoundError()
    {
        // Arrange
        var request = new QuestionRequest("Updated", new List<string> { "A1" });
        _unitOfWorkMock.Setup(u => u.Questions.GetAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Question?)null);

        // Act
        var result = await _sut.UpdateAsync(1, 1, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(QuestionErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task UpdateAsync_WhenDuplicateContentExists_ReturnsDuplicateContentError()
    {
        // Arrange
        var request = new QuestionRequest("Duplicate", new List<string> { "A1" });
        var question = new Question { Id = 1, PollId = 1, Content = "Old" };

        _unitOfWorkMock.Setup(u => u.Questions.GetAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(question);
        _unitOfWorkMock.Setup(u => u.Questions.ExistsAsync(It.IsAny<Expression<Func<Question, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.UpdateAsync(1, 1, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(QuestionErrors.DuplicateContent.Code, result.Error.Code);
    }

    [Fact]
    public async Task ToggleStatusAsync_WhenQuestionNotFound_ReturnsQuestionNotFoundError()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.Questions.ToggleStatusAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Common.Abstractions.Result)null!);

        // Act
        var result = await _sut.ToggleStatusAsync(1, 1);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(QuestionErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task ToggleStatusAsync_WhenQuestionExists_TogglesAndInvalidatesCache()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.Questions.ToggleStatusAsync(1, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Domain.Common.Abstractions.Result.Success());

        // Act
        var result = await _sut.ToggleStatusAsync(1, 1);

        // Assert
        Assert.True(result.IsSuccess);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync("questions:poll:1:1", It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync("questions:poll:1:all", It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync("questions:poll:1:available", It.IsAny<CancellationToken>()), Times.Once);
    }
}
