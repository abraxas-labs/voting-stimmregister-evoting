// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Voting.Lib.DmDoc;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Document;

namespace Voting.Stimmregister.EVoting.Adapter.Document;

public class DocumatrixPdfService : IPdfService
{
    private readonly IDmDocService _dmDoc;

    public DocumatrixPdfService(IDmDocService dmDoc)
    {
        _dmDoc = dmDoc;
    }

    public Task<Stream> RenderPdf<T>(string templateKey, T data, CancellationToken ct)
        => _dmDoc.FinishAsPdf(templateKey, data, ct: ct);
}
