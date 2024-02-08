// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.Extensions.DependencyInjection;
using Voting.Lib.Common;
using Voting.Stimmregister.EVoting.WebService.Configuration;
using Voting.Stimmregister.EVoting.WebService.Exceptions;

namespace Voting.Stimmregister.EVoting.WebService.DependencyInjection;

/// <summary>
/// Service collection extensions to register WebService services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the WebService services to DI container.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <param name="appConfig">The application configuration which will be added as Singleton.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddWebServiceServices(this IServiceCollection services, AppConfig appConfig)
    {
        services.AddSingleton(appConfig);
        services.AddSingleton<AttributeValidator>();
        services.AddScoped<EVotingExceptionFilterAttribute>();

        return services;
    }
}
