using Application.DTOs.Requests.Polls;
using Application.Validators.Polls;
using Xunit;

namespace Application.UnitTests.Validators.Polls;

public class PollRequestValidatorTests
{
    private readonly PollRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenRequestIsValid_ReturnsSuccess()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        var request = new PollRequest("Customer Survey", "Survey summary here", today, today.AddDays(7));

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")] // Less than 3
    public void Validate_WhenTitleIsTooShortOrEmpty_ReturnsError(string title)
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        var request = new PollRequest(title, "Valid Summary", today, today.AddDays(7));

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PollRequest.Title));
    }

    [Fact]
    public void Validate_WhenTitleExceedsMaxLength_ReturnsError()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        var longTitle = new string('a', 101);
        var request = new PollRequest(longTitle, "Valid Summary", today, today.AddDays(7));

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PollRequest.Title));
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public void Validate_WhenSummaryIsTooShortOrEmpty_ReturnsError(string summary)
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        var request = new PollRequest("Valid Title", summary, today, today.AddDays(7));

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PollRequest.Summary));
    }

    [Fact]
    public void Validate_WhenStartsAtIsInThePast_ReturnsError()
    {
        // Arrange
        var yesterday = DateOnly.FromDateTime(DateTime.Today).AddDays(-1);
        var request = new PollRequest("Valid Title", "Valid Summary", yesterday, yesterday.AddDays(7));

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PollRequest.StartsAt));
    }

    [Fact]
    public void Validate_WhenEndsAtIsBeforeStartsAt_ReturnsError()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);
        var request = new PollRequest("Valid Title", "Valid Summary", today.AddDays(5), today.AddDays(2));

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PollRequest.EndsAt));
    }
}
