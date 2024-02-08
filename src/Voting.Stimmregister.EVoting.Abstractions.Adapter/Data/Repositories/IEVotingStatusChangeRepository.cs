// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Database.Repositories;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Abstractions.Adapter.Data.Repositories;

/// <summary>
/// Repository for e-voting status change table.
/// </summary>
public interface IEVotingStatusChangeRepository : IDbRepository<DbContext, EVotingStatusChangeEntity>
{
    /// <summary>
    /// Gets an active status change by the person's AHVN13.
    /// </summary>
    /// <param name="ahvn13">The AHVN13.</param>
    /// <returns>The active status change for the person, if one exists.</returns>
    Task<EVotingStatusChangeEntity?> GetActiveByAhvn13(long ahvn13);

    /// <summary>
    /// Gets active status changes that do not have document attached.
    /// </summary>
    /// <param name="batchSize">The maximum amount of status changes to retrieve.</param>
    /// <returns>Status changes that do not have a document attached.</returns>
    Task<List<EVotingStatusChangeEntity>> GetWithMissingDocuments(int batchSize);

    /// <summary>
    /// Gets a status change that is due for delivery.
    /// </summary>
    /// <param name="maxDocumentDate">The maximum age of the generated status change document.</param>
    /// <returns>A status change that is due for delivery, if one exists.</returns>
    Task<EVotingStatusChangeEntity?> GetNextForDelivery(DateTime maxDocumentDate);
}
