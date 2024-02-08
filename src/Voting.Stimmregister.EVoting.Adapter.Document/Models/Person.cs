// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Stimmregister.EVoting.Domain.Enums;

namespace Voting.Stimmregister.EVoting.Adapter.Document.Models;

public class Person
{
    public Sex Sex { get; set; }

    public string OfficialName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public short MunicipalityBfs { get; set; }

    public Address? Address { get; set; }
}
