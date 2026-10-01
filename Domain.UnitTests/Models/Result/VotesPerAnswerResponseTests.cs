using Domain.Models.Result;
using Xunit;

namespace Domain.UnitTests.Models.Result;

public class VotesPerAnswerResponseTests
{
    [Fact]
    public void Constructor_WhenInstantiated_StoresAssignedValues()
    {
        // Arrange
        const string expectedAnswer = "Option 1";
        const int expectedCount = 42;

        // Act
        var response = new VotesPerAnswerResponse(expectedAnswer, expectedCount);

        // Assert
        Assert.Equal(expectedAnswer, response.Answer);
        Assert.Equal(expectedCount, response.Count);
    }

    [Fact]
    public void Equals_WhenRecordsHaveSameValues_ReturnsTrue()
    {
        // Arrange
        var response1 = new VotesPerAnswerResponse("Option A", 10);
        var response2 = new VotesPerAnswerResponse("Option A", 10);

        // Act & Assert
        Assert.Equal(response1, response2);
        Assert.True(response1 == response2);
    }

    [Fact]
    public void Equals_WhenRecordsHaveDifferentValues_ReturnsFalse()
    {
        // Arrange
        var response1 = new VotesPerAnswerResponse("Option A", 10);
        var response2 = new VotesPerAnswerResponse("Option A", 20);

        // Act & Assert
        Assert.NotEqual(response1, response2);
        Assert.True(response1 != response2);
    }
}
