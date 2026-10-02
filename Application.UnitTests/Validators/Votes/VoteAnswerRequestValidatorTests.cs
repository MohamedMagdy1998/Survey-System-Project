using Application.DTOs.Requests.Votes;
using Application.Validators.Votes;
using Xunit;

namespace Application.UnitTests.Validators.Votes;

public class VoteAnswerRequestValidatorTests
{
    private readonly VoteAnswerRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenRequestIsValid_ReturnsSuccess()
    {
        // Arrange
        var request = new VoteAnswerRequest(QuestionId: 1, AnswerId: 2);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenQuestionIdIsZeroOrNegative_ReturnsError(int questionId)
    {
        // Arrange
        var request = new VoteAnswerRequest(QuestionId: questionId, AnswerId: 2);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(VoteAnswerRequest.QuestionId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WhenAnswerIdIsZeroOrNegative_ReturnsError(int answerId)
    {
        // Arrange
        var request = new VoteAnswerRequest(QuestionId: 1, AnswerId: answerId);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(VoteAnswerRequest.AnswerId));
    }
}
