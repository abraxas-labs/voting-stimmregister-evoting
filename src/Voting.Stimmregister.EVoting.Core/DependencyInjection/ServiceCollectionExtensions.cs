// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.Extensions.DependencyInjection;
using Voting.Lib.Scheduler;
using Voting.Stimmregister.EVoting.Abstractions.Core.Services;
using Voting.Stimmregister.EVoting.Core.Configuration;
using Voting.Stimmregister.EVoting.Core.HostedServices;
using Voting.Stimmregister.EVoting.Core.Services;
using Voting.Stimmregister.EVoting.Domain.Configuration;

namespace Voting.Stimmregister.EVoting.Core.DependencyInjection;

/// <summary>
/// Service collection extensions to register Core services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the core services to DI container.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <param name="eVotingConfig">The e-voting configuration.</param>
    /// <param name="documentGeneratorConfig">The document generator configuration.</param>
    /// <param name="documentDeliveryConfig">The document delivery configuration.</param>
    /// <param name="metricsConfig">The metrics configuration.</param>
    /// <param name="machineConfig">The machine configuration.</param>
    /// <param name="rateLimitConfig">The rate limit configuration.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddCoreServices(
        this IServiceCollection services,
        EVotingConfig eVotingConfig,
        DocumentGeneratorConfig documentGeneratorConfig,
        ICronJobConfig documentDeliveryConfig,
        JobConfig metricsConfig,
        MachineConfig machineConfig,
        RateLimitConfig rateLimitConfig)
    {
        services
            .AddSingleton(eVotingConfig)
            .AddSingleton(documentGeneratorConfig)
            .AddSingleton(machineConfig)
            .AddSingleton(rateLimitConfig)
            .AddScoped<ITracingService, TracingService>()
            .AddScoped<IEVoterService, EVotingService>()
            .AddScoped<IRegistrationService, EVotingService>()
            .AddScoped<IRateLimitService, RateLimitService>()
            .AddScoped<DocumentGeneratorWorker>()
            .AddScoped<DocumentDeliveryWorker>()
            .AddScoped<MetricsWorker>();

        services.AddCronJob<DocumentGeneratorJob>(documentGeneratorConfig);
        services.AddCronJob<DocumentDeliveryJob>(documentDeliveryConfig);
        services.AddScheduledJob<MetricsJob>(metricsConfig);

        return services;
    }
}
