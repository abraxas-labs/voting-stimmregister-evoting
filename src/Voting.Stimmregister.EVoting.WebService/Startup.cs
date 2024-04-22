// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Globalization;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prometheus;
using Voting.Lib.Common.DependencyInjection;
using Voting.Lib.Rest.Middleware;
using Voting.Lib.Rest.Swagger.DependencyInjection;
using Voting.Stimmregister.EVoting.Adapter.Data;
using Voting.Stimmregister.EVoting.Adapter.Data.DependencyInjection;
using Voting.Stimmregister.EVoting.Adapter.Document.DependencyInjection;
using Voting.Stimmregister.EVoting.Adapter.DokConnector.DependencyInjection;
using Voting.Stimmregister.EVoting.Adapter.Stimmregister.DependencyInjection;
using Voting.Stimmregister.EVoting.Core.DependencyInjection;
using Voting.Stimmregister.EVoting.Domain.Converters;
using Voting.Stimmregister.EVoting.Rest.Controller;
using Voting.Stimmregister.EVoting.WebService.Configuration;
using Voting.Stimmregister.EVoting.WebService.DependencyInjection;
using Voting.Stimmregister.EVoting.WebService.Exceptions;

namespace Voting.Stimmregister.EVoting.WebService;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
        AppConfig = configuration.Get<AppConfig>()!;
    }

    protected AppConfig AppConfig { get; }

    public virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddWebServiceServices(AppConfig);
        services.AddCoreServices(
            AppConfig.EVoting,
            AppConfig.DocumentGenerator,
            AppConfig.DocumentDelivery,
            AppConfig.Metrics,
            AppConfig.Machine,
            AppConfig.RateLimit);

        services.AddAdapterDataServices(AppConfig.Database, ConfigureDatabase);
        services.AddAdapterStimmregisterServices(AppConfig.Stimmregister, AppConfig.SecureConnect.AbraxasTenantId, AppConfig.EVotingServiceAccount);
        services.AddAdapterDocumentServices(AppConfig.Documatrix);
        services.AddAdapterDokConnector(AppConfig.DokConnector, AppConfig.SharedSecureConnect);

        services.AddCertificatePinning(AppConfig.CertificatePinning);
        services.AddVotingLibPrometheusAdapter(new() { Interval = AppConfig.PrometheusAdapterInterval });
        services.AddSystemClock();

        ConfigureHealthChecks(services.AddHealthChecks());
        ConfigureAuthentication(services.AddVotingLibIam(new() { BaseUrl = AppConfig.SecureConnectApi }));

        // without the ApplicationParts, the REST controllers are sometimes not discovered (mainly in tests)
        services
            .AddControllers(opts => opts.Filters.Add<EVotingExceptionFilterAttribute>())
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
            })
            .PartManager.ApplicationParts.Add(new AssemblyPart(typeof(RegistrationController).Assembly));

        services.AddSwaggerGenerator(_configuration);

        ConfigureGlobalFluentValidationOptions();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseMetricServer(AppConfig.MetricPort);
        app.UseHttpMetrics();

        app.UseMiddleware<Middlewares.ExceptionHandler>();
        app.UseMiddleware<Middlewares.TracingMiddleware>();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<IamLoggingHandler>();
        app.UseSerilogRequestLoggingWithTraceabilityModifiers();

        app.UseSwaggerGenerator();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapVotingHealthChecks(AppConfig.LowPriorityHealthCheckNames);
            MapEndpoints(endpoints);
        });
    }

    protected virtual void ConfigureAuthentication(AuthenticationBuilder builder)
        => builder.AddSecureConnectScheme(options =>
        {
            options.Audience = AppConfig.SecureConnect.Audience;
            options.Authority = AppConfig.SecureConnect.Authority;
            options.FetchRoleToken = true;
            options.LimitRolesToAppHeaderApps = false;
            options.ServiceAccount = AppConfig.SecureConnect.ServiceAccount;
            options.ServiceAccountPassword = AppConfig.SecureConnect.ServiceAccountPassword;
            options.ServiceAccountScopes = AppConfig.SecureConnect.ServiceAccountScopes;
        });

    protected virtual void ConfigureDatabase(DbContextOptionsBuilder db)
        => db.UseNpgsql(AppConfig.Database.ConnectionString, o => o.SetPostgresVersion(AppConfig.Database.Version));

    /// <summary>
    /// Force using german for speaking fluent validation errors for the user.
    /// </summary>
    private static void ConfigureGlobalFluentValidationOptions()
        => ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("de-DE");

    private void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapControllers();
    }

    private void ConfigureHealthChecks(IHealthChecksBuilder checks)
    {
        checks
            .AddDbContextCheck<DataContext>()
            .ForwardToPrometheus();
    }
}
