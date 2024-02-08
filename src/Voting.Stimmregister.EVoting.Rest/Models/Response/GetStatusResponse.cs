// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Stimmregister.EVoting.Domain.Enums;

namespace Voting.Stimmregister.EVoting.Rest.Models.Response;

public class GetStatusResponse : ProcessStatusResponseBase
{
    public EVotingStatus VotingStatus { get; init; }

    public VotingRight VotingRight { get; init; }
}
