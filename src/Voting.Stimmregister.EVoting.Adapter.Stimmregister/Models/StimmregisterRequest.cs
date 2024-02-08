// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmregister.EVoting.Adapter.Stimmregister.Models;

/// <summary>
/// The shared class for all Stimmregister requests.
/// Currently, all requests to the Stimmregister API share the same structure.
/// </summary>
public class StimmregisterRequest
{
    public string Ahvn13 { get; set; } = string.Empty;

    public short BfsCanton { get; set; }
}
