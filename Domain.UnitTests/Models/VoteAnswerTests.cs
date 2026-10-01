using Domain.Models;
using Xunit;

namespace Domain.UnitTests.Models;

public class VoteAnswerTests
{
    [Fact]
    public void Constructor_DefaultValues_InitializesDefaults()
    {
        // Act
        var voteAnswer = new VoteAnswer();

        // Assert
        Assert.Equal(0, voteAnswer.Id);
        Assert.Equal(0, voteAnswer.VoteId);
        Assert.Equal(0, voteAnswer.QuestionId);
        Assert.Equal(0, voteAnswer.AnswerId);
    }

    [Fact]
    public void Properties_WhenAssigned_StoreAssignedValues()
    {
        // Arrange
        var voteAnswer = new VoteAnswer();
        var vote = new Vote { Id = 10 };
        var question = new Question { Id = 20 };
        var answer = new Answer { Id = 30 };

        // Act
        voteAnswer.Id = 1;
        voteAnswer.VoteId = 10;
        voteAnswer.QuestionId = 20;
        voteAnswer.AnswerId = 30;
        voteAnswer.Vote = vote;
        voteAnswer.Question = question;
        voteAnswer.Answer = answer;

        // Assert
        Assert.Equal(1, voteAnswer.Id);
        Assert.Equal(10, voteAnswer.VoteId);
        Assert.Equal(20, voteAnswer.QuestionId);
        Assert.Equal(30, voteAnswer.AnswerId);
        Assert.Same(vote, voteAnswer.Vote);
        Assert.Same(question, voteAnswer.Question);
        Assert.Same(answer, voteAnswer.Answer);
    }
}
