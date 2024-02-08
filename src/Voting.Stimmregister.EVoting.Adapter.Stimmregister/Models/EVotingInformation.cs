// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmregister.EVoting.Adapter.Stimmregister.Models;

public class EVotingInformation
{
    public VotingStatus VotingStatus { get; init; }

    public Person? Person { get; set; }

    public int RegisteredEVotersInCanton { get; init; }

    public int RegisteredEVotersInMunicipality { get; init; }
}
