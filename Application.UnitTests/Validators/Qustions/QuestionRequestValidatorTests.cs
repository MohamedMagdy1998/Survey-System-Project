using Application.Validators.Qustions;
using Domain.Models;
using Xunit;

namespace Application.UnitTests.Validators.Qustions;

public class QuestionRequestValidatorTests
{
    private readonly QuestionRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenQuestionIsValid_ReturnsSuccess()
    {
        // Arrange
        var answer1 = new Answer { Id = 1, Content = "Ans 1" };
        var answer2 = new Answer { Id = 2, Content = "Ans 2" };
        var question = new Question
        {
            Content = "What is your satisfaction level?",
            Answers = new List<Answer> { answer1, answer2 }
        };

        // Act
        var result = _validator.Validate(question);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public void Validate_WhenContentIsTooShortOrEmpty_ReturnsError(string content)
    {
        // Arrange
        var question = new Question
        {
            Content = content,
            Answers = new List<Answer> { new() { Id = 1 }, new() { Id = 2 } }
        };

        // Act
        var result = _validator.Validate(question);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(Question.Content));
    }

    [Fact]
    public void Validate_WhenAnswersHasLessThanTwoItems_ReturnsError()
    {
        // Arrange
        var question = new Question
        {
            Content = "Valid question content?",
            Answers = new List<Answer> { new() { Id = 1 } }
        };

        // Act
        var result = _validator.Validate(question);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(Question.Answers));
    }

    [Fact]
    public void Validate_WhenAnswersContainsDuplicates_ReturnsError()
    {
        // Arrange
        var answer = new Answer { Id = 1, Content = "Same Answer" };
        var question = new Question
        {
            Content = "Valid question content?",
            Answers = new List<Answer> { answer, answer }
        };

        // Act
        var result = _validator.Validate(question);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(Question.Answers));
    }
}
