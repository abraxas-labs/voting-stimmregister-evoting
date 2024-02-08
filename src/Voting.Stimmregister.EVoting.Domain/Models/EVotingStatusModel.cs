// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Stimmregister.EVoting.Domain.Enums;

namespace Voting.Stimmregister.EVoting.Domain.Models;

public class EVotingStatusModel
{
    public EVotingStatus Status { get; set; }

    public VotingRight Right { get; set; }

    public string ProcessStatusMessage { get; set; } = string.Empty;
}
