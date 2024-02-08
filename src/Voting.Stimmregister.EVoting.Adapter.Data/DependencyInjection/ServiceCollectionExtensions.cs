// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Data.DataContexts;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Data.Repositories;
using Voting.Stimmregister.EVoting.Adapter.Data.Configuration;
using Voting.Stimmregister.EVoting.Adapter.Data.Repositories;

namespace Voting.Stimmregister.EVoting.Adapter.Data.DependencyInjection;

/// <summary>
/// Service collection extensions to register Adapter.Data services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the data services to DI container.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <param name="dataConfig">The data configuration which will be added as Singleton.</param>
    /// <param name="optionsBuilder">The db context options builder to configure additional db options.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddAdapterDataServices(
        this IServiceCollection services,
        DataConfig dataConfig,
        Action<DbContextOptionsBuilder> optionsBuilder)
    {
        services.AddDbContext<IDataContext, DataContext>(db =>
        {
            if (dataConfig.EnableDetailedErrors)
            {
                db.EnableDetailedErrors();
            }

            if (dataConfig.EnableSensitiveDataLogging)
            {
                db.EnableSensitiveDataLogging();
            }

            db.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            optionsBuilder(db);
        });

        return services
            .AddSingleton(dataConfig)
            .AddScoped<IDocumentRepository, DocumentRepository>()
            .AddScoped<IRateLimitRepository, RateLimitRepository>()
            .AddScoped<IEVotingStatusChangeRepository, EVotingStatusChangeRepository>()
            .AddVotingLibDatabase<DataContext>();
    }
}
