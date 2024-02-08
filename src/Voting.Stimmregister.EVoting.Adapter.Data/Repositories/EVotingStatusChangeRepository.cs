// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Database.Repositories;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Data.Repositories;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Adapter.Data.Repositories;

/// <inheritdoc cref="IEVotingStatusChangeRepository"/>
public class EVotingStatusChangeRepository : DbRepository<DataContext, EVotingStatusChangeEntity>, IEVotingStatusChangeRepository
{
    public EVotingStatusChangeRepository(DataContext context)
        : base(context)
    {
    }

    public Task<EVotingStatusChangeEntity?> GetActiveByAhvn13(long ahvn13)
    {
        return Query().SingleOrDefaultAsync(x => x.Person!.Ahvn13 == ahvn13 && x.Active);
    }

    public Task<List<EVotingStatusChangeEntity>> GetWithMissingDocuments(int batchSize)
    {
        var rawQuery = BuildFetchAndLockSql(batchSize, false);
        return Context.EVotingStatusChanges
            .FromSqlRaw(rawQuery)
            .Include(x => x.Person)
            .ToListAsync();
    }

    public Task<EVotingStatusChangeEntity?> GetNextForDelivery(DateTime maxDocumentDate)
    {
        var rawQuery = BuildFetchAndLockSql(1, true, maxDocumentDate);
        return Context.EVotingStatusChanges
            .FromSqlRaw(rawQuery)
            .Include(x => x.Document)
            .Include(e => e.Person)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// This builds a query to select active status changes.
    /// Found status changes are locked until the transaction ends.
    /// Locked status changes are ignored by this query, meaning two parallel queries will never return the same rows.
    /// This is ideal for parallel jobs that operate on the same rows, so that no rows are processed multiple times.
    /// </summary>
    /// <param name="batchSize">The maximum amount of status changes to return.</param>
    /// <param name="hasDocument">Whether the status change has a document.</param>
    /// <param name="maxDocumentDate">The maximum creation date of the status change document.</param>
    /// <returns>The SQL query.</returns>
    private string BuildFetchAndLockSql(int batchSize, bool hasDocument, DateTime? maxDocumentDate = null)
    {
        var idColumnName = Set.GetDelimitedColumnName(x => x.Id);
        var activeColumnName = Set.GetDelimitedColumnName(x => x.Active);

        var documentTableName = Context.Documents.GetDelimitedSchemaAndTableName();
        var statusChangeIdFkColumnName = Context.Documents.GetDelimitedColumnName(x => x.StatusChangeId);
        var documentDateColumnName = Context.Documents.GetDelimitedColumnName(x => x.CreatedAt);

        var hasDocumentQuery = hasDocument
            ? "IS NOT NULL"
            : "IS NULL";

        var documentDateQuery = maxDocumentDate.HasValue
            ? $"AND t2.{documentDateColumnName} < '{maxDocumentDate:u}' "
            : string.Empty;

        // CAREFUL: We use a raw SQL query here, so no user supplied values should be used.
        // Otherwise, SQL injection would be possible.
        // The currently used types (int, bool and DateTime) do not allow SQL injection
        // and thus do not need to be parameterized.
        return $"SELECT t1.* FROM {DelimitedSchemaAndTableName} AS t1 "
            + $"LEFT JOIN {documentTableName} AS t2 ON t2.{statusChangeIdFkColumnName} = t1.{idColumnName} "
            + $"WHERE {activeColumnName} = TRUE "
            + $"AND {statusChangeIdFkColumnName} {hasDocumentQuery} "
            + documentDateQuery
            + $"LIMIT {batchSize} "
            + "FOR UPDATE OF t1 SKIP LOCKED";
    }
}
