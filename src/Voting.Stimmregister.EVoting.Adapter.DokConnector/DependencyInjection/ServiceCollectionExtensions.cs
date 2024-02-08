// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.Extensions.DependencyInjection;
using Voting.Lib.Iam.ServiceTokenHandling;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.DokConnector;
using Voting.Stimmregister.EVoting.Adapter.DokConnector.Configuration;

#if DEBUG
using Voting.Stimmregister.EVoting.Adapter.DokConnector.Mocks;
#endif

namespace Voting.Stimmregister.EVoting.Adapter.DokConnector.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAdapterDokConnector(
        this IServiceCollection services,
        DokConnectorConfig dokConnectorConfig,
        SecureConnectServiceAccountOptions serviceAccountOptions)
    {
#if DEBUG
        if (dokConnectorConfig.EnableMock)
        {
            return services.AddSingleton<IDokConnectorService, DokConnectorServiceMock>();
        }
#endif

        services
            .AddSingleton(dokConnectorConfig)
            .AddScoped<IDokConnectorService, DokConnectorService>()
            .AddEaiDokConnector(dokConnectorConfig)
            .AddSecureConnectServiceToken(serviceAccountOptions);

        return services;
    }
}
