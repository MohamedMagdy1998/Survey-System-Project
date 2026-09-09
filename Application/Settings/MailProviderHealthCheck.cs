using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Settings;

public class MailProviderHealthCheck(IOptions<MailSettings> mailSettings) : IHealthCheck
{
    private readonly IOptions<MailSettings> MailSettings = mailSettings;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var smtp = new SmtpClient();
            smtp.Connect(MailSettings.Value.Host, MailSettings.Value.Port, SecureSocketOptions.StartTls, cancellationToken);
            smtp.Authenticate(MailSettings.Value.Mail, MailSettings.Value.Password, cancellationToken);

            return await Task.FromResult(HealthCheckResult.Healthy());

        }
        catch (Exception ex)
        {

            return await Task.FromResult(HealthCheckResult.Unhealthy($"{ex}"));
        }
    }
}