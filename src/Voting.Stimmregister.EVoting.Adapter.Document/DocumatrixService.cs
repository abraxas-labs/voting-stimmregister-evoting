// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Voting.Lib.Common;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Document;
using Voting.Stimmregister.EVoting.Adapter.Document.Configuration;
using Voting.Stimmregister.EVoting.Adapter.Document.Mapping;
using Voting.Stimmregister.EVoting.Adapter.Document.Models;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Adapter.Document;

public class DocumatrixService : IDocumatrixService
{
    private static readonly CultureInfo SwissCulture = new("de-CH");

    private readonly IPdfService _pdfService;
    private readonly DmDocConfig _dmDocConfig;
    private readonly IClock _clock;

    public DocumatrixService(IPdfService pdfService, DmDocConfig dmDocConfig, IClock clock)
    {
        _pdfService = pdfService;
        _dmDocConfig = dmDocConfig;
        _clock = clock;
    }

    public Task<Stream> RenderRegisteredPdf(PersonEntity person, string templateSuffix, CancellationToken ct)
    {
        var templateBag = new TemplateBag
        {
            EVotingInformation = TemplateMapper.MapToEVotingInformation(person, true, FormatDateNow()),
        };
        return RenderPdf(BuildTemplateKey(_dmDocConfig.RegisteredTemplateKey, templateSuffix), templateBag, ct);
    }

    public Task<Stream> RenderUnregisteredPdf(PersonEntity person, string templateSuffix, CancellationToken ct)
    {
        var templateBag = new TemplateBag
        {
            EVotingInformation = TemplateMapper.MapToEVotingInformation(person, false, FormatDateNow()),
        };
        return RenderPdf(BuildTemplateKey(_dmDocConfig.UnregisteredTemplateKey, templateSuffix), templateBag, ct);
    }

    private Task<Stream> RenderPdf(string templateKey, TemplateBag templateBag, CancellationToken ct) =>
        _pdfService.RenderPdf(templateKey, templateBag, ct);

    private string BuildTemplateKey(string baseKey, string suffix)
        => baseKey + suffix;

    private string FormatDateNow()
        => _clock.UtcNow.ToString("d. MMMM yyyy", SwissCulture);
}
