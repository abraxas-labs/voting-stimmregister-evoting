// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Database.Models;
using Voting.Stimmregister.EVoting.Domain.Enums;

namespace Voting.Stimmregister.EVoting.Domain.Models;

// Note: Persons are not unique. Multiple entries may exists for the same AHVN13 number.
public class PersonEntity : BaseEntity
{
    public long Ahvn13 { get; set; }

    public bool AllowedToVote { get; set; }

    public short MunicipalityBfs { get; set; }

    public string? Nationality { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public Sex Sex { get; set; }

    public string OfficialName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public AddressEntity Address { get; set; } = new();

    public Guid StatusChangeId { get; set; }

    public EVotingStatusChangeEntity? StatusChange { get; set; }
}
