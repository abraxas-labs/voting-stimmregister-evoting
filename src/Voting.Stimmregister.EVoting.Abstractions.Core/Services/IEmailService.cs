// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading;
using System.Threading.Tasks;
using Voting.Lib.UserNotifications;

namespace Voting.Stimmregister.EVoting.Abstractions.Core.Services;

public interface IEmailService
{
    Task Send(SmtpConfig config, UserNotification userNotification, CancellationToken ct);
}
