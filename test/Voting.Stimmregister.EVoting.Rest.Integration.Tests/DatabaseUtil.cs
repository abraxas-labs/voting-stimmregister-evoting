// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Stimmregister.EVoting.Adapter.Data;

namespace Voting.Stimmregister.EVoting.Rest.Integration.Tests;

public static class DatabaseUtil
{
    private static bool _migrated;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "EF1002:Risk of vulnerability to SQL injection.", Justification = "Only hardened table names used.")]
    public static async Task Truncate(DataContext db)
    {
        // on the first run, we migrate the database to ensure the same structure as the "real" DB
        if (!_migrated)
        {
            await db.Database.MigrateAsync();
            _migrated = true;
        }

        // truncating tables is much faster than recreating the database
        var tableNames = db.Model.GetEntityTypes().Select(m => $@"""{m.GetTableName()}""");
        await db.Database.ExecuteSqlRawAsync($"TRUNCATE {string.Join(",", tableNames)} CASCADE");
    }
}
