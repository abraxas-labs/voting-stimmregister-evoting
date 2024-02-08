// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Scheduler;

namespace Voting.Stimmregister.EVoting.Core.Configuration;

public class DocumentGeneratorConfig : CronJobConfig
{
    /// <summary>
    /// Gets or sets the batch size of documents inserted into the database at once.
    /// Higher values use a lot more memory (since more objects are created in the same batch).
    /// Lower value in turn take much longer to process.
    /// </summary>
    public int DocumentInsertBatchSize { get; set; } = 20;
}
