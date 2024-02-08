// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Abstractions.Adapter.Data.DataContexts;

/// <summary>
/// Database data context abstraction.
/// </summary>
public interface IDataContext
{
    /// <summary>
    /// Gets the Registrations table.
    /// </summary>
    DbSet<EVotingStatusChangeEntity> EVotingStatusChanges { get; }

    /// <summary>
    /// Gets the Documents table.
    /// </summary>
    DbSet<DocumentEntity> Documents { get; }

    /// <summary>
    /// Gets the PersonData table.
    /// </summary>
    DbSet<PersonEntity> Persons { get; }

    /// <summary>
    /// Gets the RateLimits table.
    /// </summary>
    DbSet<RateLimitEntity> RateLimits { get; }

    Task SaveChangesAsync();

    /// <summary>
    /// Starts a new database transaction.
    /// </summary>
    /// <param name="isolationLevel">The isolation level.</param>
    /// <returns>The created database transaction.</returns>
    Task<IDbContextTransaction> BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
}
