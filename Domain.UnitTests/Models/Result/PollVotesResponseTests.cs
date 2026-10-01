using Application.DTOs.Responses.Result;
using Xunit;

namespace Domain.UnitTests.Models.Result;

public class PollVotesResponseTests
{
    [Fact]
    public void Constructor_WhenInstantiated_StoresAssignedValues()
    {
        // Arrange
        const string expectedTitle = "Customer Feedback";
        var votes = new List<VoteResponse>();

        // Act
        var response = new PollVotesResponse(expectedTitle, votes);

        // Assert
        Assert.Equal(expectedTitle, response.Title);
        Assert.Same(votes, response.Votes);
    }

    [Fact]
    public void Equals_WhenRecordsHaveSameValues_ReturnsTrue()
    {
        // Arrange
        var votes = new List<VoteResponse>();
        var response1 = new PollVotesResponse("Title A", votes);
        var response2 = new PollVotesResponse("Title A", votes);

        // Act & Assert
        Assert.Equal(response1, response2);
        Assert.True(response1 == response2);
    }
}
