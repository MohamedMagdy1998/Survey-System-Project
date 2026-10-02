using Application.Services_Implementations;
using Application.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Moq;
using Xunit;

namespace Application.UnitTests.Services_Implementations;

public class EmailServiceTests
{
    private readonly Mock<ILogger<EmailService>> _loggerMock = new();

    [Fact]
    public async Task SendEmailAsync_WhenRecipientEmailIsInvalid_ThrowsParseException()
    {
        // Arrange
        var mailSettings = Microsoft.Extensions.Options.Options.Create(new MailSettings
        {
            Mail = "admin@surveybasket.com",
            DisplayName = "Survey Basket",
            Host = "127.0.0.1",
            Port = 1,
            Password = "pass"
        });

        var service = new EmailService(mailSettings, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAnyAsync<ParseException>(() =>
            service.SendEmailAsync("invalid email with spaces", "Test Subject", "<p>Message</p>"));
    }

    [Fact]
    public async Task SendEmailAsync_WhenSmtpHostIsUnreachable_ThrowsException()
    {
        // Arrange
        var mailSettings = Microsoft.Extensions.Options.Options.Create(new MailSettings
        {
            Mail = "admin@surveybasket.com",
            DisplayName = "Survey Basket",
            Host = "127.0.0.1",
            Port = 1,
            Password = "pass"
        });

        var service = new EmailService(mailSettings, _loggerMock.Object);

        // Act & Assert
        await Assert.ThrowsAnyAsync<Exception>(() =>
            service.SendEmailAsync("recipient@example.com", "Test Subject", "<p>Message</p>"));
    }
}
