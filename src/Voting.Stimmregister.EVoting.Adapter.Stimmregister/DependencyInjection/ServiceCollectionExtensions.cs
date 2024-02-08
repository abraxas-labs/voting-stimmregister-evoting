// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.Extensions.DependencyInjection;
using Voting.Lib.Iam.ServiceTokenHandling;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Stimmregister;
using Voting.Stimmregister.EVoting.Adapter.Stimmregister.Configuration;

namespace Voting.Stimmregister.EVoting.Adapter.Stimmregister.DependencyInjection;

/// <summary>
/// Service collection extensions to register Adapter.Data services.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAdapterStimmregisterServices(
        this IServiceCollection services,
        StimmregisterConfig stimmregisterConfig,
        string abraxasTenantId,
        SecureConnectServiceAccountOptions serviceAccountOptions)
    {
        services.AddHttpClient<IStimmregisterService, StimmregisterService>(httpClient =>
            {
                httpClient.BaseAddress = new Uri(stimmregisterConfig.Url);
                httpClient.DefaultRequestHeaders.Add("x-tenant", abraxasTenantId);
            })
            .AddSecureConnectServiceToken(serviceAccountOptions);

        return services;
    }
}
