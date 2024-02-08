// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Voting.Stimmregister.EVoting.Abstractions.Adapter.DokConnector;

public interface IDokConnectorService
{
    Task Upload(string fileName, Stream content, CancellationToken ct);
}
