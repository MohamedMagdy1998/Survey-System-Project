using Domain.Models;
using Xunit;

namespace Domain.UnitTests.Models;

public class VoteTests
{
    [Fact]
    public void Constructor_DefaultValues_InitializesDefaultsAndEmptyCollection()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var vote = new Vote();
        var after = DateTime.UtcNow;

        // Assert
        Assert.Equal(0, vote.Id);
        Assert.Equal(0, vote.PollId);
        Assert.Equal(string.Empty, vote.UserId);
        Assert.True(vote.SubmittedOn >= before && vote.SubmittedOn <= after);
        Assert.NotNull(vote.VoteAnswers);
        Assert.Empty(vote.VoteAnswers);
    }

    [Fact]
    public void Properties_WhenAssigned_StoreAssignedValues()
    {
        // Arrange
        var vote = new Vote();
        var poll = new Poll { Id = 2 };
        var user = new ApplicationUser { Id = "user-abc" };
        var submissionTime = DateTime.UtcNow.AddMinutes(-10);
        var answers = new List<VoteAnswer> { new() { Id = 1 } };

        // Act
        vote.Id = 7;
        vote.PollId = 2;
        vote.UserId = "user-abc";
        vote.SubmittedOn = submissionTime;
        vote.Poll = poll;
        vote.User = user;
        vote.VoteAnswers = answers;

        // Assert
        Assert.Equal(7, vote.Id);
        Assert.Equal(2, vote.PollId);
        Assert.Equal("user-abc", vote.UserId);
        Assert.Equal(submissionTime, vote.SubmittedOn);
        Assert.Same(poll, vote.Poll);
        Assert.Same(user, vote.User);
        Assert.Same(answers, vote.VoteAnswers);
    }
}
