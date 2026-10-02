using Application.Services_Implementations;
using Domain.Contracts;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Application.UnitTests.Common.TestHelpers;
using Moq;
using Xunit;

namespace Application.UnitTests.Services_Implementations;

public class NotificationServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<IEmailSender> _emailSenderMock = new();
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Origin"] = "https://example.com";
        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(httpContext);

        _sut = new NotificationService(
            _unitOfWorkMock.Object,
            _userManagerMock.Object,
            _httpContextAccessorMock.Object,
            _emailSenderMock.Object);
    }

    [Fact]
    public async Task SendNewPollsNotification_WhenPollIdProvidedAndPublished_SendsNotificationEmail()
    {
        // Arrange
        var pollId = 10;
        var poll = new Poll
        {
            Id = pollId,
            Title = "Tech Trends",
            IsPublished = true,
            EndsAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
        };

        var users = new List<ApplicationUser>
        {
            new() { Id = "u1", FirstName = "Alice", Email = "alice@example.com" }
        }.BuildMock();

        _unitOfWorkMock.Setup(u => u.Polls.GetAsync(pollId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(poll);
        _userManagerMock.Setup(m => m.Users).Returns(users);

        // Act
        await _sut.SendNewPollsNotification(pollId);

        // Assert
        _emailSenderMock.Verify(
            e => e.SendEmailAsync(
                "alice@example.com",
                It.Is<string>(s => s.Contains("Tech Trends")),
                It.Is<string>(b => b.Contains("Alice"))),
            Times.Once);
    }

    [Fact]
    public async Task SendNewPollsNotification_WhenPollIdProvidedAndNotPublished_DoesNotSendEmail()
    {
        // Arrange
        var pollId = 10;
        var poll = new Poll
        {
            Id = pollId,
            Title = "Draft Poll",
            IsPublished = false
        };

        var users = new List<ApplicationUser>
        {
            new() { Id = "u1", FirstName = "Alice", Email = "alice@example.com" }
        }.BuildMock();

        _unitOfWorkMock.Setup(u => u.Polls.GetAsync(pollId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(poll);
        _userManagerMock.Setup(m => m.Users).Returns(users);

        // Act
        await _sut.SendNewPollsNotification(pollId);

        // Assert
        _emailSenderMock.Verify(
            e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task SendNewPollsNotification_WhenPollIdNull_SendsNotificationForPollsStartingToday()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activePoll = new Poll
        {
            Id = 1,
            Title = "Daily Poll",
            IsPublished = true,
            StartsAt = today,
            EndsAt = today.AddDays(1)
        };
        var futurePoll = new Poll
        {
            Id = 2,
            Title = "Future Poll",
            IsPublished = true,
            StartsAt = today.AddDays(5)
        };

        var users = new List<ApplicationUser>
        {
            new() { Id = "u1", FirstName = "Bob", Email = "bob@example.com" }
        }.BuildMock();

        _unitOfWorkMock.Setup(u => u.Polls.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Poll> { activePoll, futurePoll });
        _userManagerMock.Setup(m => m.Users).Returns(users);

        // Act
        await _sut.SendNewPollsNotification();

        // Assert
        _emailSenderMock.Verify(
            e => e.SendEmailAsync(
                "bob@example.com",
                It.Is<string>(s => s.Contains("Daily Poll")),
                It.IsAny<string>()),
            Times.Once);

        _emailSenderMock.Verify(
            e => e.SendEmailAsync(
                "bob@example.com",
                It.Is<string>(s => s.Contains("Future Poll")),
                It.IsAny<string>()),
            Times.Never);
    }
}
