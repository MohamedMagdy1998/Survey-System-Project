using Application.Settings;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;

namespace Application.UnitTests.Settings;

public class MailProviderHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WhenCannotConnectToHost_ReturnsUnhealthyResult()
    {
        // Arrange
        var mailSettings = Microsoft.Extensions.Options.Options.Create(new MailSettings
        {
            Host = "127.0.0.1",
            Port = 1, // Port 1 won't be listening to SMTP
            Mail = "test@example.com",
            Password = "password"
        });

        var healthCheck = new MailProviderHealthCheck(mailSettings);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.NotNull(result.Description);
    }
}
