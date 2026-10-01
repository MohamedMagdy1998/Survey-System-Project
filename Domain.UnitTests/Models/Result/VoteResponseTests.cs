using Application.DTOs.Responses.Result;
using Xunit;

namespace Domain.UnitTests.Models.Result;

public class VoteResponseTests
{
    [Fact]
    public void Constructor_WhenInstantiated_StoresAssignedValues()
    {
        // Arrange
        const string voterName = "Alice";
        var voteDate = new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);
        var answers = new List<QuestionAnswerResponse>
        {
            new("Q1", "A1")
        };

        // Act
        var response = new VoteResponse(voterName, voteDate, answers);

        // Assert
        Assert.Equal(voterName, response.VoterName);
        Assert.Equal(voteDate, response.VoteDate);
        Assert.Same(answers, response.SelectedAnswers);
    }

    [Fact]
    public void Equals_WhenRecordsHaveSameValues_ReturnsTrue()
    {
        // Arrange
        var voteDate = new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);
        var answers = new List<QuestionAnswerResponse>();
        var response1 = new VoteResponse("Alice", voteDate, answers);
        var response2 = new VoteResponse("Alice", voteDate, answers);

        // Act & Assert
        Assert.Equal(response1, response2);
        Assert.True(response1 == response2);
    }
}
