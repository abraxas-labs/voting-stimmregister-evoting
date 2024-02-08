// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Voting.Lib.DmDoc;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Document;
using Voting.Stimmregister.EVoting.Adapter.Document.Configuration;
using Voting.Stimmregister.EVoting.Adapter.Document.Mapping;
using Voting.Stimmregister.EVoting.Adapter.Document.Models;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Adapter.Document;

public class DocumatrixService : IDocumatrixService
{
    private readonly IDmDocService _dmDoc;
    private readonly DmDocConfig _dmDocConfig;

    public DocumatrixService(IDmDocService dmDoc, DmDocConfig dmDocConfig)
    {
        _dmDoc = dmDoc;
        _dmDocConfig = dmDocConfig;
    }

    public Task<Stream> RenderRegisteredPdf(PersonEntity person, CancellationToken ct)
    {
        var templateBag = new TemplateBag
        {
            EVotingInformation = TemplateMapper.MapToEVotingInformation(person, true),
        };
        return RenderPdf(_dmDocConfig.RegisteredTemplateKey, templateBag, ct);
    }

    public Task<Stream> RenderUnregisteredPdf(PersonEntity person, CancellationToken ct)
    {
        var templateBag = new TemplateBag
        {
            EVotingInformation = TemplateMapper.MapToEVotingInformation(person, false),
        };
        return RenderPdf(_dmDocConfig.UnregisteredTemplateKey, templateBag, ct);
    }

    private Task<Stream> RenderPdf(string templateKey, TemplateBag templateBag, CancellationToken ct) =>
        _dmDoc.FinishAsPdf(templateKey, templateBag, ct: ct);
}
