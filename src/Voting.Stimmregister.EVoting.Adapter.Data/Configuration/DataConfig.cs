// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Npgsql;

namespace Voting.Stimmregister.EVoting.Adapter.Data.Configuration;

public class DataConfig
{
    public string ConnectionString
    {
        get
        {
            return new NpgsqlConnectionStringBuilder
            {
                Host = Host,
                Port = Port,
                Username = User,
                Password = Pass,
                Database = Name,
                IncludeErrorDetail = EnableDetailedErrors,
                Timeout = Timeout,
            }.ToString();
        }
    }

    public string Host { get; set; } = string.Empty;

    public ushort Port { get; set; } = 5432;

    public string User { get; set; } = string.Empty;

    public string Pass { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the target database.
    /// Required for Postgres when database version is lower v14 and Npgsql greater or equal v8.
    /// </summary>
    public Version? Version { get; set; } = null;

    public int Timeout { get; set; } = 15;

    public bool EnableSensitiveDataLogging { get; set; }

    public bool EnableDetailedErrors { get; set; }
}
