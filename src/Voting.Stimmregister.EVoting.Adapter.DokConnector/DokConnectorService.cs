// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Voting.Lib.DokConnector.Service;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.DokConnector;
using Voting.Stimmregister.EVoting.Adapter.DokConnector.Configuration;

namespace Voting.Stimmregister.EVoting.Adapter.DokConnector;

public class DokConnectorService : IDokConnectorService
{
    private readonly IDokConnector _dokConnector;
    private readonly string _messageType;

    public DokConnectorService(IDokConnector dokConnector, DokConnectorConfig dokConnectorConfig)
    {
        _dokConnector = dokConnector;
        _messageType = dokConnectorConfig.MessageType;
    }

    public Task Upload(string fileName, Stream content, CancellationToken ct)
    {
        return _dokConnector.Upload(_messageType, fileName, content, ct);
    }
}
