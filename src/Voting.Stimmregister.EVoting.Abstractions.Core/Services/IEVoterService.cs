// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading;
using System.Threading.Tasks;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Abstractions.Core.Services;

public interface IEVoterService
{
    Task<EVotingStatusModel> GetEVotingStatus(PersonIdentification personIdentification, CancellationToken ct);
}
