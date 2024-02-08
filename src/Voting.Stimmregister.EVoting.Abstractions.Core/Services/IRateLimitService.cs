// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading;
using System.Threading.Tasks;
using Voting.Lib.Common;

namespace Voting.Stimmregister.EVoting.Abstractions.Core.Services;

/// <summary>
/// A service responsible for rate limiting.
/// </summary>
public interface IRateLimitService
{
    /// <summary>
    /// Checks that the rate limit for the AHVN13 has not been reached and increases it.
    /// </summary>
    /// <param name="ahvn13">The AHVN13 to check.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task CheckAndIncreaseRateLimit(Ahvn13 ahvn13, CancellationToken ct);
}
