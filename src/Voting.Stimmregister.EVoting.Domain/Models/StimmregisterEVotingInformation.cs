// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Stimmregister.EVoting.Domain.Enums;

namespace Voting.Stimmregister.EVoting.Domain.Models;

public class EVotingInformation
{
    public EVotingInformation(Person person)
    {
        Person = person;
    }

    public EVotingStatus Status { get; set; }

    public Person Person { get; }

    public int RegisteredEVotersInCanton { get; set; }

    public int RegisteredEVotersInMunicipality { get; set; }
}
