// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Riok.Mapperly.Abstractions;
using Voting.Stimmregister.EVoting.Domain.Models;
using EVotingInformation = Voting.Stimmregister.EVoting.Adapter.Document.Models.EVotingInformation;
using Person = Voting.Stimmregister.EVoting.Adapter.Document.Models.Person;

namespace Voting.Stimmregister.EVoting.Adapter.Document.Mapping;

[Mapper]
internal static partial class TemplateMapper
{
    internal static EVotingInformation MapToEVotingInformation(PersonEntity personEntity, bool registered)
    {
        return new EVotingInformation
        {
            Person = MapToPerson(personEntity),
            EVotingRegistered = registered,
        };
    }

    [MapperIgnoreSource(nameof(PersonEntity.Ahvn13))]
    [MapperIgnoreSource(nameof(PersonEntity.AllowedToVote))]
    [MapperIgnoreSource(nameof(PersonEntity.Nationality))]
    [MapperIgnoreSource(nameof(PersonEntity.DateOfBirth))]
    [MapperIgnoreSource(nameof(PersonEntity.StatusChangeId))]
    [MapperIgnoreSource(nameof(PersonEntity.StatusChange))]
    [MapperIgnoreSource(nameof(PersonEntity.Id))]
    internal static partial Person MapToPerson(PersonEntity personEntity);
}
