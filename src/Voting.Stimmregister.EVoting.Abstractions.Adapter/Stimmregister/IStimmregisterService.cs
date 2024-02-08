// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading;
using System.Threading.Tasks;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Abstractions.Adapter.Stimmregister;

public interface IStimmregisterService
{
    Task RegisterAsync(PersonIdentification personIdentification, CancellationToken ct);

    Task UnregisterAsync(PersonIdentification personIdentification, CancellationToken ct);

    Task<EVotingInformation> GetEVotingInformationAsync(PersonIdentification personIdentification, CancellationToken ct);
}
