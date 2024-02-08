// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Common;
using Voting.Stimmregister.EVoting.Domain.Enums;

namespace Voting.Stimmregister.EVoting.Domain.Models;

public class Person
{
    public Ahvn13? Ahvn13 { get; set; }

    public bool AllowedToVote { get; init; }

    public short MunicipalityBfs { get; init; }

    public string? Nationality { get; init; }

    public DateOnly DateOfBirth { get; init; }

    public Sex Sex { get; set; }

    public string OfficialName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public Address? Address { get; set; }
}
