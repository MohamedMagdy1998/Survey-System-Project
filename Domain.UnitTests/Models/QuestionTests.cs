using Domain.Models;
using Xunit;

namespace Domain.UnitTests.Models;

public class QuestionTests
{
    [Fact]
    public void Constructor_DefaultValues_InitializesDefaultsAndEmptyCollections()
    {
        // Act
        var question = new Question();

        // Assert
        Assert.Equal(0, question.Id);
        Assert.Equal(string.Empty, question.Content);
        Assert.Equal(0, question.PollId);
        Assert.True(question.IsActive);
        Assert.NotNull(question.Answers);
        Assert.Empty(question.Answers);
        Assert.NotNull(question.Votes);
        Assert.Empty(question.Votes);
    }

    [Fact]
    public void Inheritance_WhenInstantiated_DerivesFromAuditableEntity()
    {
        // Act
        var question = new Question();

        // Assert
        Assert.IsAssignableFrom<AuditableEntity>(question);
    }

    [Fact]
    public void Properties_WhenAssigned_StoreAssignedValues()
    {
        // Arrange
        var question = new Question();
        var poll = new Poll { Id = 5, Title = "Tech Poll" };
        var answers = new List<Answer> { new() { Id = 1, Content = "Yes" } };
        var voteAnswers = new List<VoteAnswer> { new() { Id = 1 } };

        // Act
        question.Id = 1;
        question.Content = "Do you prefer C# over other languages?";
        question.PollId = 5;
        question.IsActive = false;
        question.Poll = poll;
        question.Answers = answers;
        question.Votes = voteAnswers;

        // Assert
        Assert.Equal(1, question.Id);
        Assert.Equal("Do you prefer C# over other languages?", question.Content);
        Assert.Equal(5, question.PollId);
        Assert.False(question.IsActive);
        Assert.Same(poll, question.Poll);
        Assert.Same(answers, question.Answers);
        Assert.Same(voteAnswers, question.Votes);
    }
}
