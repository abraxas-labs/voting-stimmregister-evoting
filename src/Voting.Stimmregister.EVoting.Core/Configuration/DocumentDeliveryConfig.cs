// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Scheduler;

namespace Voting.Stimmregister.EVoting.Core.Configuration;

/// <summary>
/// Configuration for a per-tenant document delivery cron job.
/// Extends <see cref="CronJobConfig"/> with the canton BFS number that identifies
/// the tenant this job instance is responsible for.
/// </summary>
public class DocumentDeliveryConfig : CronJobConfig
{
    /// <summary>
    /// Gets or sets the canton BFS number used to identify
    /// the tenant whose documents this job delivers.
    /// </summary>
    public short CantonBfs { get; set; }
}
