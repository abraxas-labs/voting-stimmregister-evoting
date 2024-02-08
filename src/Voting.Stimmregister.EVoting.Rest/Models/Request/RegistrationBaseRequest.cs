// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmregister.EVoting.Rest.Models.Request;

public class RegistrationBaseRequest
{
    public string Ahvn13 { get; set; } = string.Empty;

    public short BfsCanton { get; set; }

    public DateOnly DateOfBirth { get; set; }
}
