// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.Extensions.DependencyInjection;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Document;
using Voting.Stimmregister.EVoting.Adapter.Document.Configuration;

#if DEBUG
using Voting.Stimmregister.EVoting.Adapter.Document.Mocks;
#endif

namespace Voting.Stimmregister.EVoting.Adapter.Document.DependencyInjection;

/// <summary>
/// Service collection extensions to register Adapter.Document services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the document services to DI container.
    /// </summary>
    /// <param name="services">The services collection.</param>
    /// <param name="dmDocConfig">The documatrix config.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddAdapterDocumentServices(this IServiceCollection services, DmDocConfig dmDocConfig)
    {
#if DEBUG
        if (dmDocConfig.EnableMock)
        {
            return services.AddSingleton<IDocumatrixService, DocumatrixServiceMock>();
        }
#endif

        return services
            .AddDmDoc(dmDocConfig)
            .AddSingleton(dmDocConfig)
            .AddScoped<IDocumatrixService, DocumatrixService>();
    }
}
