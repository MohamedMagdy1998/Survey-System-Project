using Domain.Models;
using Xunit;

namespace Domain.UnitTests.Models;

public class PollTests
{
    [Fact]
    public void Constructor_DefaultValues_InitializesDefaultsAndEmptyCollections()
    {
        // Act
        var poll = new Poll();

        // Assert
        Assert.Equal(0, poll.Id);
        Assert.Equal(string.Empty, poll.Title);
        Assert.Equal(string.Empty, poll.Summary);
        Assert.False(poll.IsPublished);
        Assert.Equal(default, poll.StartsAt);
        Assert.Equal(default, poll.EndsAt);
        Assert.NotNull(poll.Questions);
        Assert.Empty(poll.Questions);
        Assert.NotNull(poll.Votes);
        Assert.Empty(poll.Votes);
    }

    [Fact]
    public void Inheritance_WhenInstantiated_DerivesFromAuditableEntity()
    {
        // Act
        var poll = new Poll();

        // Assert
        Assert.IsAssignableFrom<AuditableEntity>(poll);
    }

    [Fact]
    public void Properties_WhenAssigned_StoreAssignedValues()
    {
        // Arrange
        var poll = new Poll();
        var startDate = new DateOnly(2026, 10, 1);
        var endDate = new DateOnly(2026, 10, 31);
        var questions = new List<Question> { new() { Id = 1, Content = "Q1" } };
        var votes = new List<Vote> { new() { Id = 1 } };

        // Act
        poll.Id = 10;
        poll.Title = "Employee Satisfaction Survey";
        poll.Summary = "Annual feedback survey";
        poll.IsPublished = true;
        poll.StartsAt = startDate;
        poll.EndsAt = endDate;
        poll.Questions = questions;
        poll.Votes = votes;

        // Assert
        Assert.Equal(10, poll.Id);
        Assert.Equal("Employee Satisfaction Survey", poll.Title);
        Assert.Equal("Annual feedback survey", poll.Summary);
        Assert.True(poll.IsPublished);
        Assert.Equal(startDate, poll.StartsAt);
        Assert.Equal(endDate, poll.EndsAt);
        Assert.Same(questions, poll.Questions);
        Assert.Same(votes, poll.Votes);
    }
}
