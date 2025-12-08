// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.DokConnector;

namespace Voting.Stimmregister.EVoting.Rest.Integration.Tests.Mocks;

public class DokConnectorServiceMock : IDokConnectorService
{
    public List<(string FileName, string MessageType)> UploadedFiles { get; } = [];

    public Task Upload(string fileName, Stream content, string messageType, CancellationToken ct)
    {
        UploadedFiles.Add((fileName, messageType));
        return Task.CompletedTask;
    }
}
