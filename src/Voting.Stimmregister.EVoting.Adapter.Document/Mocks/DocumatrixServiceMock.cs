// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

#if DEBUG
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Document;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Adapter.Document.Mocks;

public class DocumatrixServiceMock : IDocumatrixService
{
    private static readonly string DummyRegisteredPdfPath = Path.Combine(Path.GetDirectoryName(typeof(DocumatrixServiceMock).Assembly.Location)!, "Mocks/evoting_brief_anmeldung.pdf");
    private static readonly string DummyUnregisteredPdfPath = Path.Combine(Path.GetDirectoryName(typeof(DocumatrixServiceMock).Assembly.Location)!, "Mocks/evoting_brief_abmeldung.pdf");

    public Task<Stream> RenderRegisteredPdf(PersonEntity person, CancellationToken ct) =>
        Task.FromResult<Stream>(File.OpenRead(DummyRegisteredPdfPath));

    public Task<Stream> RenderUnregisteredPdf(PersonEntity person, CancellationToken ct) =>
        Task.FromResult<Stream>(File.OpenRead(DummyUnregisteredPdfPath));
}
#endif
