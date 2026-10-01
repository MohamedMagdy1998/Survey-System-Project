using Domain.Models;
using Xunit;

namespace Domain.UnitTests.Models;

public class AnswerTests
{
    [Fact]
    public void Constructor_DefaultValues_InitializesDefaults()
    {
        // Act
        var answer = new Answer();

        // Assert
        Assert.Equal(0, answer.Id);
        Assert.Equal(string.Empty, answer.Content);
        Assert.Equal(0, answer.QuestionId);
        Assert.True(answer.IsActive);
    }

    [Fact]
    public void Inheritance_WhenInstantiated_DerivesFromAuditableEntity()
    {
        // Act
        var answer = new Answer();

        // Assert
        Assert.IsAssignableFrom<AuditableEntity>(answer);
    }

    [Fact]
    public void Properties_WhenAssigned_StoreAssignedValues()
    {
        // Arrange
        var answer = new Answer();
        var question = new Question { Id = 3, Content = "Sample question" };

        // Act
        answer.Id = 15;
        answer.Content = "Strongly Agree";
        answer.QuestionId = 3;
        answer.IsActive = false;
        answer.Question = question;

        // Assert
        Assert.Equal(15, answer.Id);
        Assert.Equal("Strongly Agree", answer.Content);
        Assert.Equal(3, answer.QuestionId);
        Assert.False(answer.IsActive);
        Assert.Same(question, answer.Question);
    }
}
