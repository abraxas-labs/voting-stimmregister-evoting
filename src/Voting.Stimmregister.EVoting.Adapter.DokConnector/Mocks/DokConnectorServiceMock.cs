// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

#if DEBUG
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.DokConnector;

namespace Voting.Stimmregister.EVoting.Adapter.DokConnector.Mocks;

public class DokConnectorServiceMock : IDokConnectorService
{
    private static readonly string OutDirectoryPath =
        Path.Join(
            Path.GetTempPath(),
            "voting-stimmregister-e-voting");

    private readonly ILogger<DokConnectorServiceMock> _logger;

    public DokConnectorServiceMock(ILogger<DokConnectorServiceMock> logger)
    {
        _logger = logger;
        Directory.CreateDirectory(OutDirectoryPath);
    }

    public async Task Upload(string fileName, Stream content, CancellationToken ct)
    {
        _logger.LogDebug("Write {fileName} to {OutDirectoryPath}", fileName, OutDirectoryPath);
        var targetFilePath = Path.Combine(OutDirectoryPath, fileName);
        await using var fs = File.OpenWrite(targetFilePath);
        await content.CopyToAsync(fs, ct);
    }
}
#endif
