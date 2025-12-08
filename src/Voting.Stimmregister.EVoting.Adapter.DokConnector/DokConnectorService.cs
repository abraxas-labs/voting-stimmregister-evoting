// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Voting.Lib.DokConnector.Service;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.DokConnector;

namespace Voting.Stimmregister.EVoting.Adapter.DokConnector;

public class DokConnectorService : IDokConnectorService
{
    private readonly IDokConnector _dokConnector;

    public DokConnectorService(IDokConnector dokConnector)
    {
        _dokConnector = dokConnector;
    }

    public Task Upload(string fileName, Stream content, string messageType, CancellationToken ct)
    {
        return _dokConnector.Upload(messageType, fileName, content, ct);
    }
}
