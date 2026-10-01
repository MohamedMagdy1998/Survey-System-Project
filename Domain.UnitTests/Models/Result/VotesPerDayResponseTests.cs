using Application.DTOs.Responses.Result;
using Xunit;

namespace Domain.UnitTests.Models.Result;

public class VotesPerDayResponseTests
{
    [Fact]
    public void Constructor_WhenInstantiated_StoresAssignedValues()
    {
        // Arrange
        var date = new DateOnly(2026, 10, 1);
        const int numberOfVotes = 150;

        // Act
        var response = new VotesPerDayResponse(date, numberOfVotes);

        // Assert
        Assert.Equal(date, response.Date);
        Assert.Equal(numberOfVotes, response.NumberOfVotes);
    }

    [Fact]
    public void Equals_WhenRecordsHaveSameValues_ReturnsTrue()
    {
        // Arrange
        var date = new DateOnly(2026, 10, 1);
        var response1 = new VotesPerDayResponse(date, 50);
        var response2 = new VotesPerDayResponse(date, 50);

        // Act & Assert
        Assert.Equal(response1, response2);
        Assert.True(response1 == response2);
    }

    [Fact]
    public void Equals_WhenRecordsHaveDifferentValues_ReturnsFalse()
    {
        // Arrange
        var date1 = new DateOnly(2026, 10, 1);
        var date2 = new DateOnly(2026, 10, 2);
        var response1 = new VotesPerDayResponse(date1, 50);
        var response2 = new VotesPerDayResponse(date2, 50);

        // Act & Assert
        Assert.NotEqual(response1, response2);
        Assert.True(response1 != response2);
    }
}
