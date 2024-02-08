// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Hosting;
using Serilog;
using Voting.Lib.Testing;

namespace Voting.Stimmregister.EVoting.Rest.Integration.Tests;

public class TestApplicationFactory : BaseTestApplicationFactory<TestStartup>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder
            .UseEnvironment("Test")
            .UseSolutionRelativeContentRoot("src/Voting.Stimmregister.EVoting.WebService");
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return base.CreateHostBuilder()
            .UseSerilog((context, _, configuration) => configuration.ReadFrom.Configuration(context.Configuration))
            .ConfigureAppConfiguration((_, config) =>
            {
                // we deploy our config with the docker image, no need to watch for changes
                foreach (var source in config.Sources.OfType<JsonConfigurationSource>())
                {
                    source.ReloadOnChange = false;
                }
            });
    }
}
