// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmregister.EVoting.Rest.Models.Request;

public class VerifyEmailRequest
{
    public string Code { get; set; } = string.Empty;

    public short BfsCanton { get; set; }
}
