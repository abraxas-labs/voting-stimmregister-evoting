// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Stimmregister.EVoting.Domain.Enums;

namespace Voting.Stimmregister.EVoting.Domain.Models;

public class EVotingUnregisterResultModel
{
    public ProcessStatusCode ProcessStatusCode { get; set; } = ProcessStatusCode.Unknown;

    public string ProcessStatusMessage { get; set; } = string.Empty;
}
