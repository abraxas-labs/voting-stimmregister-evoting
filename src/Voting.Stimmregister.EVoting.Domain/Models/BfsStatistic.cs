// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmregister.EVoting.Domain.Models;

public class BfsStatistic
{
    public string Bfs { get; set; } = string.Empty;

    public int VoterTotalCount { get; set; }

    public int EVoterTotalCount { get; set; }
}
