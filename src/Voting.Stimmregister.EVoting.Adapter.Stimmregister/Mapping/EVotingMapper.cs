// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Riok.Mapperly.Abstractions;
using Voting.Lib.Common;
using Voting.Stimmregister.EVoting.Adapter.Stimmregister.Models;
using DomainModels = Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Adapter.Stimmregister.Mapping;

[Mapper]
internal static partial class EVotingMapper
{
    [MapProperty(nameof(EVotingInformation.VotingStatus), nameof(DomainModels.EVotingInformation.Status))]
    internal static partial DomainModels.EVotingInformation MapEVotingInfo(EVotingInformation response);

    private static DomainModels.Person MapPerson(Person person)
    {
        var mapped = MapPersonBase(person);
        mapped.Ahvn13 = Ahvn13.Parse(person.Ahvn13);
        return mapped;
    }

    [MapperIgnoreSource(nameof(Person.Ahvn13))]
    [MapperIgnoreTarget(nameof(DomainModels.Person.Ahvn13))]
    private static partial DomainModels.Person MapPersonBase(Person person);
}
