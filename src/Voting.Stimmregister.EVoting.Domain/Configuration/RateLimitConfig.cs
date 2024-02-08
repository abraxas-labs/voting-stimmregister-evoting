// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmregister.EVoting.Domain.Configuration;

/// <summary>
/// Config for rate limiting.
/// </summary>
public class RateLimitConfig
{
    /// <summary>
    /// Gets or sets the rate limit per AHVN13 and day.
    /// All actions (get status, register, unregister) counts towards this rate limit.
    /// </summary>
    public int RateLimitPerAhvn13PerDay { get; set; }
}
