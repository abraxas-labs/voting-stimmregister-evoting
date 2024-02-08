// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmregister.EVoting.Adapter.Stimmregister.Models;

public class Person
{
    public long Ahvn13 { get; set; }

    public bool AllowedToVote { get; set; }

    public short MunicipalityBfs { get; set; }

    public string? Nationality { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public Sex Sex { get; set; }

    public string OfficialName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public Address? Address { get; set; }
}
