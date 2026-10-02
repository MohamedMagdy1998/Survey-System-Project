using Application.DTOs.Requests.Votes;
using Application.Validators.Votes;
using Xunit;

namespace Application.UnitTests.Validators.Votes;

public class VoteRequestValidatorTests
{
    private readonly VoteRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenRequestIsValid_ReturnsSuccess()
    {
        // Arrange
        var answers = new List<VoteAnswerRequest>
        {
            new(QuestionId: 1, AnswerId: 10),
            new(QuestionId: 2, AnswerId: 20)
        };
        var request = new VoteRequest(answers);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenAnswersIsEmpty_ReturnsError()
    {
        // Arrange
        var request = new VoteRequest(new List<VoteAnswerRequest>());

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(VoteRequest.Answers));
    }

    [Fact]
    public void Validate_WhenAnAnswerItemIsInvalid_ReturnsError()
    {
        // Arrange
        var answers = new List<VoteAnswerRequest>
        {
            new(QuestionId: 1, AnswerId: 10),
            new(QuestionId: 0, AnswerId: 20) // Invalid QuestionId
        };
        var request = new VoteRequest(answers);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }
}
