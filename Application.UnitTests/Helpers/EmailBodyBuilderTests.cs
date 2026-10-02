using Application.Helpers;
using Xunit;

namespace Application.UnitTests.Helpers;

public class EmailBodyBuilderTests
{
    [Fact]
    public void GenerateEmailBody_WhenTemplateExists_ReplacesPlaceholdersAndReturnsContent()
    {
        // Arrange
        var templateName = "EmailConfirmation";
        var placeholders = new Dictionary<string, string>
        {
            { "{{name}}", "John Doe" },
            { "{{action_url}}", "https://surveybasket.com/confirm?code=123" }
        };

        // Act
        var result = EmailBodyBuilder.GenerateEmailBody(templateName, placeholders);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("John Doe", result);
        Assert.Contains("https://surveybasket.com/confirm?code=123", result);
        Assert.DoesNotContain("{{name}}", result);
        Assert.DoesNotContain("{{action_url}}", result);
    }

    [Fact]
    public void GenerateEmailBody_WhenPollNotificationTemplate_ReplacesAllPlaceholders()
    {
        // Arrange
        var templateName = "PollNotification";
        var placeholders = new Dictionary<string, string>
        {
            { "{{name}}", "Jane Doe" },
            { "{{pollTill}}", "Customer Feedback" },
            { "{{endDate}}", "2026-10-31" },
            { "{{url}}", "https://surveybasket.com/polls/start/1" }
        };

        // Act
        var result = EmailBodyBuilder.GenerateEmailBody(templateName, placeholders);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Jane Doe", result);
        Assert.Contains("Customer Feedback", result);
        Assert.Contains("2026-10-31", result);
        Assert.Contains("https://surveybasket.com/polls/start/1", result);
    }

    [Fact]
    public void GenerateEmailBody_WhenTemplateDoesNotExist_ThrowsFileNotFoundException()
    {
        // Arrange
        var templateName = "UnknownNonExistentTemplate";
        var placeholders = new Dictionary<string, string>();

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() =>
            EmailBodyBuilder.GenerateEmailBody(templateName, placeholders));
    }
}
