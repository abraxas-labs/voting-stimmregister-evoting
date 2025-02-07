// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Riok.Mapperly.Abstractions;
using Voting.Stimmregister.EVoting.Domain.Enums;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Core.Mappings;

[Mapper]
internal static partial class EVotingMapper
{
    internal static EVotingStatusChangeEntity MapToStatusChange(EVotingInformation response, string contextId, DateTime timestamp)
    {
        var mapped = MapToStatusChangeBase(response);
        mapped.EVotingRegistered = response.Status == EVotingStatus.Registered;
        mapped.ContextId = contextId;
        mapped.CreatedAt = timestamp;
        return mapped;
    }

    [UserMapping(Default = true)]
    private static PersonEntity MapPerson(Person person)
    {
        var mapped = MapPersonBase(person);
        mapped.Ahvn13 = person.Ahvn13!.ToNumber();
        return mapped;
    }

    [MapperIgnoreSource(nameof(EVotingInformation.Status))]
    [MapperIgnoreSource(nameof(EVotingInformation.CantonStatistic))]
    [MapperIgnoreSource(nameof(EVotingInformation.MunicipalityStatistic))]
    [MapperIgnoreTarget(nameof(EVotingStatusChangeEntity.Id))]
    [MapperIgnoreTarget(nameof(EVotingStatusChangeEntity.EVotingRegistered))]
    [MapperIgnoreTarget(nameof(EVotingStatusChangeEntity.CreatedAt))]
    [MapperIgnoreTarget(nameof(EVotingStatusChangeEntity.ContextId))]
    [MapperIgnoreTarget(nameof(EVotingStatusChangeEntity.Document))]
    [MapperIgnoreTarget(nameof(EVotingStatusChangeEntity.Active))]
    private static partial EVotingStatusChangeEntity MapToStatusChangeBase(EVotingInformation response);

    [MapperIgnoreSource(nameof(Person.Ahvn13))]
    [MapperIgnoreTarget(nameof(PersonEntity.Id))]
    [MapperIgnoreTarget(nameof(PersonEntity.Ahvn13))]
    [MapperIgnoreTarget(nameof(PersonEntity.StatusChangeId))]
    [MapperIgnoreTarget(nameof(PersonEntity.StatusChange))]
    private static partial PersonEntity MapPersonBase(Person person);
}
