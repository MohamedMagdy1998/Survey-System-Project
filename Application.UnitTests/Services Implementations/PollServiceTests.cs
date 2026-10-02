using Application.DTOs.Requests.Polls;
using Application.DTOs.Responses.Polls;
using Application.Services_Implementations;
using Application.Services_Interfaces;
using Domain.Common.Abstractions.Errors;
using Domain.Contracts;
using Domain.Models;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace Application.UnitTests.Services_Implementations;

public class PollServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly PollService _sut;

    public PollServiceTests()
    {
        InitializeHangfireStorage();
        _sut = new PollService(_unitOfWorkMock.Object, _notificationServiceMock.Object);
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
    public async Task GetAllAsync_WhenPollsExist_ReturnsSuccessWithPollList()
    {
        // Arrange
        var polls = new List<Poll>
        {
            new() { Id = 1, Title = "Poll 1", Summary = "Summary 1" }
        };

        _unitOfWorkMock.Setup(u => u.Polls.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(polls);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
    }

    [Fact]
    public async Task GetAllAsync_WhenPollsNull_ReturnsNotFoundError()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.Polls.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Poll>)null!);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task GetCurrentAsync_WhenPollsExist_ReturnsSuccessWithCurrentPolls()
    {
        // Arrange
        var polls = new List<Poll>
        {
            new() { Id = 2, Title = "Current Poll", Summary = "Summary 2" }
        };

        _unitOfWorkMock.Setup(u => u.Polls.GetCurrentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(polls);

        // Act
        var result = await _sut.GetCurrentAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
    }

    [Fact]
    public async Task GetCurrentAsync_WhenPollsNull_ReturnsNotFoundError()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.Polls.GetCurrentAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Poll>)null!);

        // Act
        var result = await _sut.GetCurrentAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task GetAsync_WhenPollExists_ReturnsSuccessWithPoll()
    {
        // Arrange
        var poll = new Poll { Id = 1, Title = "Poll 1", Summary = "Summary 1" };
        _unitOfWorkMock.Setup(u => u.Polls.GetAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(poll);

        // Act
        var result = await _sut.GetAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Poll 1", result.Value.Title);
    }

    [Fact]
    public async Task GetAsync_WhenPollDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.Polls.GetAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Poll?)null);

        // Act
        var result = await _sut.GetAsync(99);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task AddAsync_WhenTitleIsDuplicate_ReturnsDuplicateTitleError()
    {
        // Arrange
        var request = new PollRequest("Duplicate Title", "Summary", DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddDays(5)));

        _unitOfWorkMock.Setup(u => u.Polls.ExistsAsync(It.IsAny<Expression<Func<Poll, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.DuplicateTitle.Code, result.Error.Code);
    }

    [Fact]
    public async Task AddAsync_WhenTitleIsUnique_AddsPollAndReturnsSuccess()
    {
        // Arrange
        var request = new PollRequest("Unique Title", "Summary", DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddDays(5)));

        _unitOfWorkMock.Setup(u => u.Polls.ExistsAsync(It.IsAny<Expression<Func<Poll, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Unique Title", result.Value.Title);
        _unitOfWorkMock.Verify(u => u.Polls.AddAsync(It.IsAny<Poll>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenPollNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var request = new PollRequest("Updated Title", "Summary", DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddDays(5)));
        _unitOfWorkMock.Setup(u => u.Polls.GetAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Poll?)null);

        // Act
        var result = await _sut.UpdateAsync(1, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task UpdateAsync_WhenDuplicateTitleExists_ReturnsDuplicateTitleError()
    {
        // Arrange
        var request = new PollRequest("Duplicate Title", "Summary", DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddDays(5)));
        var currentPoll = new Poll { Id = 1, Title = "Old Title" };

        _unitOfWorkMock.Setup(u => u.Polls.GetAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentPoll);
        _unitOfWorkMock.Setup(u => u.Polls.ExistsAsync(It.IsAny<Expression<Func<Poll, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.UpdateAsync(1, request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.DuplicateTitle.Code, result.Error.Code);
    }

    [Fact]
    public async Task UpdateAsync_WhenValidRequest_UpdatesPollAndReturnsSuccess()
    {
        // Arrange
        var request = new PollRequest("New Title", "New Summary", DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddDays(5)));
        var currentPoll = new Poll { Id = 1, Title = "Old Title" };

        _unitOfWorkMock.Setup(u => u.Polls.GetAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentPoll);
        _unitOfWorkMock.Setup(u => u.Polls.ExistsAsync(It.IsAny<Expression<Func<Poll, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.UpdateAsync(1, request);

        // Assert
        Assert.True(result.IsSuccess);
        _unitOfWorkMock.Verify(u => u.Polls.UpdateAsync(currentPoll, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenPollNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.Polls.GetAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Poll?)null);

        // Act
        var result = await _sut.DeleteAsync(1);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task DeleteAsync_WhenPollExists_DeletesPollAndReturnsSuccess()
    {
        // Arrange
        var poll = new Poll { Id = 1, Title = "To Delete" };
        _unitOfWorkMock.Setup(u => u.Polls.GetAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(poll);

        // Act
        var result = await _sut.DeleteAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        _unitOfWorkMock.Verify(u => u.Polls.DeleteAsync(poll, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TogglePublishStatusAsync_WhenPollNotFound_ReturnsNotFoundError()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.Polls.GetAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Poll?)null);

        // Act
        var result = await _sut.TogglePublishStatusAsync(1);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(PollErrors.NotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task TogglePublishStatusAsync_WhenToggledToPublishedAndStartsToday_TogglesAndReturnsSuccess()
    {
        // Arrange
        var poll = new Poll
        {
            Id = 1,
            IsPublished = false,
            StartsAt = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        _unitOfWorkMock.Setup(u => u.Polls.GetAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(poll);

        // Act
        var result = await _sut.TogglePublishStatusAsync(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(poll.IsPublished);
        _unitOfWorkMock.Verify(u => u.Polls.UpdateAsync(poll, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
