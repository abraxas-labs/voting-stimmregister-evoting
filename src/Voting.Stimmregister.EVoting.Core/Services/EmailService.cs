// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Voting.Lib.UserNotifications;
using Voting.Stimmregister.EVoting.Abstractions.Core.Services;
using Voting.Stimmregister.EVoting.Domain.Diagnostics;

namespace Voting.Stimmregister.EVoting.Core.Services;

public class EmailService(ILogger<EmailService> logger) : IEmailService
{
    public async Task Send(SmtpConfig config, UserNotification userNotification, CancellationToken ct)
    {
        DiagnosticsConfig.IncreaseEmailSendAttempts();
        logger.LogInformation("Sending email...");

        try
        {
            var userNotificationSender = new SmtpUserNotificationSender(config);
            await userNotificationSender.Send(userNotification, ct);
            logger.LogInformation("Sent email");
            DiagnosticsConfig.IncreaseSentEmails();
        }
        catch (Exception ex)
        {
            DiagnosticsConfig.IncreaseEmailErrors();
            logger.LogWarning(ex, "Sending email failed");
            throw;
        }
    }
}
