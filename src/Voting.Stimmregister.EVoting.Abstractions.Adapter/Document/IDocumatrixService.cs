// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Abstractions.Adapter.Document;

public interface IDocumatrixService
{
    Task<Stream> RenderRegisteredPdf(PersonEntity person, CancellationToken ct);

    Task<Stream> RenderUnregisteredPdf(PersonEntity person, CancellationToken ct);
}
