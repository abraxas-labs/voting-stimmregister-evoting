// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Net.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmregister.EVoting.Rest.Integration.Tests.Mocks;
using Voting.Stimmregister.EVoting.WebService;

namespace Voting.Stimmregister.EVoting.Rest.Integration.Tests;

public class TestStartup : Startup
{
    public TestStartup(IConfiguration configuration)
        : base(configuration)
    {
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services
            .AddVotingLibIamMocks()
            .RemoveHostedServices()
            .AddSingleton<IHttpClientFactory, HttpClientFactoryMock>();
    }

    protected override void ConfigureAuthentication(AuthenticationBuilder builder)
        => builder.AddMockedSecureConnectScheme();
}
