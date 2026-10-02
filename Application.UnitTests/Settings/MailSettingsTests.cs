using Application.Settings;
using Xunit;

namespace Application.UnitTests.Settings;

public class MailSettingsTests
{
    [Fact]
    public void Constructor_DefaultValues_InitializesExpectedDefaults()
    {
        // Act
        var settings = new MailSettings();

        // Assert
        Assert.Equal(string.Empty, settings.Mail);
        Assert.Equal(string.Empty, settings.DisplayName);
        Assert.Equal(string.Empty, settings.Password);
        Assert.Equal(string.Empty, settings.Host);
        Assert.Equal(0, settings.Port);
    }

    [Fact]
    public void Properties_WhenAssigned_StoresAssignedValues()
    {
        // Act
        var settings = new MailSettings
        {
            Mail = "support@surveybasket.com",
            DisplayName = "Survey Basket Support",
            Password = "SecurePassword123!",
            Host = "smtp.surveybasket.com",
            Port = 587
        };

        // Assert
        Assert.Equal("support@surveybasket.com", settings.Mail);
        Assert.Equal("Survey Basket Support", settings.DisplayName);
        Assert.Equal("SecurePassword123!", settings.Password);
        Assert.Equal("smtp.surveybasket.com", settings.Host);
        Assert.Equal(587, settings.Port);
    }
}
