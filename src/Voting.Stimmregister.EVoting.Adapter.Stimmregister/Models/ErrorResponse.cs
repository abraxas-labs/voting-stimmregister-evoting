// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Stimmregister.EVoting.Domain.Enums;

namespace Voting.Stimmregister.EVoting.Adapter.Stimmregister.Models;

/// <summary>
/// The response returned by Stimmregister in case of an error.
/// </summary>
public class ErrorResponse
{
    public ProcessStatusCode ProcessStatusCode { get; set; }

    public string ProcessStatusMessage { get; set; } = string.Empty;
}
