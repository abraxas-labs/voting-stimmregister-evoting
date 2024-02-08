// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace Voting.Stimmregister.EVoting.Adapter.Data;

/// <summary>
/// Data context factory used to create ef core migrations.
/// This class is compiled only for development configuration and will be removed for 'Release' configuration.
/// </summary>
internal class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    /// <summary>
    /// Gets a dummy connection string to create ef core migrations.
    /// </summary>
    private string ConnectionString => new NpgsqlConnectionStringBuilder
    {
        Host = "localhost",
        Username = "user",
        Password = "password",
        Database = "voting-stimmregister-evoting",
    }.ToString();

    public DataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        optionsBuilder.UseNpgsql(ConnectionString);

        return new DataContext(optionsBuilder.Options);
    }
}
