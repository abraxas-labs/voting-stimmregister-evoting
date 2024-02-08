// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using Voting.Lib.Common.Net;
using Voting.Lib.Iam.ServiceTokenHandling;
using Voting.Lib.Scheduler;
using Voting.Stimmregister.EVoting.Adapter.Data.Configuration;
using Voting.Stimmregister.EVoting.Adapter.Document.Configuration;
using Voting.Stimmregister.EVoting.Adapter.DokConnector.Configuration;
using Voting.Stimmregister.EVoting.Adapter.Stimmregister.Configuration;
using Voting.Stimmregister.EVoting.Core.Configuration;
using Voting.Stimmregister.EVoting.Domain.Configuration;

namespace Voting.Stimmregister.EVoting.WebService.Configuration;

public class AppConfig
{
    /// <summary>
    /// Gets or sets the CORS config options used within the <see cref="Lib.Common.DependencyInjection.ApplicationBuilderExtensions"/>
    /// to configure the CORS middleware from <see cref="Microsoft.AspNetCore.Builder.CorsMiddlewareExtensions"/>.
    /// </summary>
    public CorsConfig Cors { get; set; } = new CorsConfig();

    /// <summary>
    /// Gets or sets the Ports configuration with the listening ports for the application.
    /// </summary>
    public PortConfig Ports { get; set; } = new PortConfig();

    /// <summary>
    /// Gets or sets the port configuration for the metric endpoint.
    /// </summary>
    public ushort MetricPort { get; set; } = 9090;

    /// <summary>
    /// Gets or sets the Database configuration.
    /// </summary>
    public DataConfig Database { get; set; } = new DataConfig();

    /// <summary>
    /// Gets or sets the identity provider configuration.
    /// </summary>
    public VotingIamConfig SecureConnect { get; set; } = new VotingIamConfig();

    /// <summary>
    /// Gets or sets the service account.
    /// </summary>
    public SecureConnectServiceAccountOptions EVotingServiceAccount { get; set; } = new();

    /// <summary>
    /// Gets or sets the identity provider api.
    /// </summary>
    public Uri? SecureConnectApi { get; set; }

    public SecureConnectServiceAccountOptions SharedSecureConnect { get; set; } = new();

    /// <summary>
    /// Gets or sets the certificate pinning configuration.
    /// </summary>
    public CertificatePinningConfig CertificatePinning { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether detailed errors are enabled. Should not be enabled in production environments,
    /// as this could expose information about the internals of this service.
    /// </summary>
    public bool EnableDetailedErrors { get; set; }

    /// <summary>
    /// Gets or sets a list of paths where language headers are getting ignored.
    /// </summary>
    public HashSet<string> LanguageHeaderIgnoredPaths { get; set; } = new()
    {
        "/healthz",
        "/metrics",
    };

    /// <summary>
    /// Gets or sets a time span for the prometheus adapter interval.
    /// </summary>
    public TimeSpan PrometheusAdapterInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the health check names of all health checks which are considered as non mission-critical
    /// (if any of them is unhealthy the system may still operate but in a degraded state).
    /// These health checks are monitored separately.
    /// </summary>
    public HashSet<string> LowPriorityHealthCheckNames { get; set; } = new();

    /// <summary>
    /// Gets or sets the E-Voting settings.
    /// </summary>
    public EVotingConfig EVoting { get; set; } = new();

    /// <summary>
    /// Gets or sets the URL to the VOTING Stimmregister service.
    /// </summary>
    public StimmregisterConfig Stimmregister { get; set; } = new();

    public DmDocConfig Documatrix { get; set; } = new() { DataSerializationFormat = Lib.DmDoc.Configuration.DmDocDataSerializationFormat.Xml };

    public DocumentGeneratorConfig DocumentGenerator { get; set; } = new();

    public CronJobConfig DocumentDelivery { get; set; } = new();

    public JobConfig Metrics { get; set; } = new() { Interval = TimeSpan.FromMinutes(10) };

    public MachineConfig Machine { get; set; } = new();

    public DokConnectorConfig DokConnector { get; set; } = new();

    public RateLimitConfig RateLimit { get; set; } = new();
}
