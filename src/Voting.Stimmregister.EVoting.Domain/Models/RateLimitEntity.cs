// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Database.Models;

namespace Voting.Stimmregister.EVoting.Domain.Models;

public class RateLimitEntity : BaseEntity
{
    /// <summary>
    /// Gets or sets the AHVN13 for which this rate limit counts.
    /// </summary>
    public long Ahvn13 { get; set; }

    /// <summary>
    /// Gets or sets the day for which this rate limit counts.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Gets or sets the amount of actions performed.
    /// </summary>
    public int ActionCount { get; set; }
}
