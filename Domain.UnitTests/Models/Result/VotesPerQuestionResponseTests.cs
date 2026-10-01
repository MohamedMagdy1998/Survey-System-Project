using Application.DTOs.Responses.Result;
using Domain.Models.Result;
using Xunit;

namespace Domain.UnitTests.Models.Result;

public class VotesPerQuestionResponseTests
{
    [Fact]
    public void Constructor_WhenInstantiated_StoresAssignedValues()
    {
        // Arrange
        const string expectedQuestion = "Which framework do you prefer?";
        var selectedAnswers = new List<VotesPerAnswerResponse>
        {
            new("ASP.NET Core", 120),
            new("Node.js", 45)
        };

        // Act
        var response = new VotesPerQuestionResponse(expectedQuestion, selectedAnswers);

        // Assert
        Assert.Equal(expectedQuestion, response.Question);
        Assert.Same(selectedAnswers, response.SelectedAnswers);
    }

    [Fact]
    public void Equals_WhenRecordsHaveSameValues_ReturnsTrue()
    {
        // Arrange
        var selectedAnswers = new List<VotesPerAnswerResponse>();
        var response1 = new VotesPerQuestionResponse("Question A", selectedAnswers);
        var response2 = new VotesPerQuestionResponse("Question A", selectedAnswers);

        // Act & Assert
        Assert.Equal(response1, response2);
        Assert.True(response1 == response2);
    }
}
