using Application.DTOs.Responses.Result;
using Xunit;

namespace Domain.UnitTests.Models.Result;

public class QuestionAnswerResponseTests
{
    [Fact]
    public void Constructor_WhenInstantiated_StoresAssignedValues()
    {
        // Arrange
        const string expectedQuestion = "What is your favorite language?";
        const string expectedAnswer = "C#";

        // Act
        var response = new QuestionAnswerResponse(expectedQuestion, expectedAnswer);

        // Assert
        Assert.Equal(expectedQuestion, response.Question);
        Assert.Equal(expectedAnswer, response.Answer);
    }

    [Fact]
    public void Equals_WhenRecordsHaveSameValues_ReturnsTrue()
    {
        // Arrange
        var response1 = new QuestionAnswerResponse("Question A", "Answer A");
        var response2 = new QuestionAnswerResponse("Question A", "Answer A");

        // Act & Assert
        Assert.Equal(response1, response2);
        Assert.True(response1 == response2);
    }

    [Fact]
    public void Equals_WhenRecordsHaveDifferentValues_ReturnsFalse()
    {
        // Arrange
        var response1 = new QuestionAnswerResponse("Question A", "Answer A");
        var response2 = new QuestionAnswerResponse("Question B", "Answer B");

        // Act & Assert
        Assert.NotEqual(response1, response2);
        Assert.True(response1 != response2);
    }
}
