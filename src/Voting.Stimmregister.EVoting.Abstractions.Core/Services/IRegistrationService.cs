// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading;
using System.Threading.Tasks;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Abstractions.Core.Services;

public interface IRegistrationService
{
    Task Register(PersonIdentification personIdentification, CancellationToken ct);

    Task Unregister(PersonIdentification personIdentification, CancellationToken ct);
}
