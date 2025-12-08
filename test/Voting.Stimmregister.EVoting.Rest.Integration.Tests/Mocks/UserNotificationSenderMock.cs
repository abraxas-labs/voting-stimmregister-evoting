// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Voting.Lib.UserNotifications;
using Voting.Stimmregister.EVoting.Abstractions.Core.Services;

namespace Voting.Stimmregister.EVoting.Rest.Integration.Tests.Mocks;

public class EmailServiceMock : IEmailService
{
    public List<UserNotification> Sent { get; } = [];

    public Task Send(SmtpConfig config, UserNotification userNotification, CancellationToken ct)
    {
        Sent.Add(userNotification);
        return Task.CompletedTask;
    }
}
