// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Voting.Stimmregister.EVoting.Abstractions.Adapter.Document;

public interface IPdfService
{
    Task<Stream> RenderPdf<T>(string templateKey, T data, CancellationToken ct);
}
