// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using Cronos;
using Voting.Lib.Common;
using Voting.Lib.UserNotifications;

namespace Voting.Stimmregister.EVoting.Domain.Configuration;

public class EVotingCustomConfig
{
    /// <summary>
    /// Gets or sets the maximum percentage of voters that are allowed to register for e-voting.
    /// </summary>
    public int MaxAllowedVotersPercent { get; set; }

    public bool RequiresEmail { get; set; }

    /// <summary>
    /// Gets or sets the email verification base URL that will be sent out via email.
    /// The user then clicks on this URL, which later leads back to the verify-email endpoint.
    /// </summary>
    public string EmailVerificationCallbackUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP config for sending emails.
    /// </summary>
    public SmtpConfig Smtp { get; set; } = new();

    public TimeSpan EmailVerificationValidity { get; set; } = TimeSpan.FromDays(1);

    public int EmailVerificationCodeLength { get; set; } = 16;

    public string TemplateSuffix { get; set; } = string.Empty;

    public string ConnectorMessageType { get; set; } = string.Empty;

    public string EVotingEnabledMunicipalities { get; set; } = string.Empty;

    public string CantonName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cron schedule for this tenant's document delivery job.
    /// </summary>
    public string DeliveryCronSchedule { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the time zone in which the delivery cron schedule should be evaluated.
    /// Defaults to Europe/Zurich.
    /// </summary>
    public string DeliveryCronTimeZone { get; set; } = DateTimeConstants.EuropeZurichTimeZoneId;

    public ICollection<short> EVotingEnabledMunicipalitiesList => EVotingEnabledMunicipalities
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(e => short.Parse(e.Trim()))
        .ToList();

    public void Validate()
    {
        if (!CronExpression.TryParse(DeliveryCronSchedule, out _))
        {
            throw new ArgumentException($"Invalid cron expression: {DeliveryCronSchedule}", nameof(DeliveryCronSchedule));
        }

        if (!RequiresEmail)
        {
            return;
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(EmailVerificationCallbackUrl);
        ArgumentNullException.ThrowIfNull(Smtp);
    }
}
