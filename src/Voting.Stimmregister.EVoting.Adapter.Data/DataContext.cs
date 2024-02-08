// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Data.DataContexts;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Adapter.Data;

public class DataContext : DbContext, IDataContext
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    /// <inheritdoc />
    public DbSet<EVotingStatusChangeEntity> EVotingStatusChanges => Set<EVotingStatusChangeEntity>();

    /// <inheritdoc />
    public DbSet<DocumentEntity> Documents => Set<DocumentEntity>();

    /// <inheritdoc />
    public DbSet<PersonEntity> Persons => Set<PersonEntity>();

    /// <inheritdoc />
    public DbSet<RateLimitEntity> RateLimits => Set<RateLimitEntity>();

    /// <inheritdoc />
    public Task<IDbContextTransaction> BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        => Database.BeginTransactionAsync(isolationLevel);

    /// <summary>
    /// Saves changes async by calling <see cref="DbContext.SaveChangesAsync"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task SaveChangesAsync() => base.SaveChangesAsync();

    /// <summary>
    /// Workaround to access the DbContext instance in the model builder classes.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
}
