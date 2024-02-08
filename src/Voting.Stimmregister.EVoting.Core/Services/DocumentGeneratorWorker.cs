// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Voting.Lib.Common;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Data.DataContexts;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Data.Repositories;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Document;
using Voting.Stimmregister.EVoting.Core.Configuration;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Core.Services;

public class DocumentGeneratorWorker
{
    private const string RegisteredFileNamePart = "anmeldung";
    private const string UnregisteredFileNamePart = "abmeldung";

    private readonly IEVotingStatusChangeRepository _statusChangeRepository;
    private readonly IDocumatrixService _documatrixService;
    private readonly IDocumentRepository _documentRepository;
    private readonly IClock _clock;
    private readonly DocumentGeneratorConfig _documentGeneratorConfig;
    private readonly MachineConfig _machineConfig;
    private readonly IDataContext _dataContext;
    private readonly ILogger<DocumentGeneratorWorker> _logger;

    public DocumentGeneratorWorker(
        IEVotingStatusChangeRepository statusChangeRepository,
        IDocumatrixService documatrixService,
        IDocumentRepository documentRepository,
        IClock clock,
        DocumentGeneratorConfig documentGeneratorConfig,
        MachineConfig machineConfig,
        IDataContext dataContext,
        ILogger<DocumentGeneratorWorker> logger)
    {
        _statusChangeRepository = statusChangeRepository;
        _documatrixService = documatrixService;
        _documentRepository = documentRepository;
        _clock = clock;
        _documentGeneratorConfig = documentGeneratorConfig;
        _machineConfig = machineConfig;
        _dataContext = dataContext;
        _logger = logger;
    }

    internal async Task Run(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && await GenerateDocuments(ct))
        {
            // generate documents (see statements in while loop)
        }
    }

    /// <summary>
    /// Generate the documents for the status changes.
    /// </summary>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>True if more documents are available to be processed. False if all documents were processed.</returns>
    private async Task<bool> GenerateDocuments(CancellationToken ct)
    {
        await using var transaction = await _dataContext.BeginTransaction();
        var statusChanges = await _statusChangeRepository.GetWithMissingDocuments(_documentGeneratorConfig.DocumentInsertBatchSize);

        if (statusChanges.Count == 0)
        {
            return false;
        }

        var documents = new List<DocumentEntity>();
        foreach (var statusChange in statusChanges)
        {
            try
            {
                documents.Add(await GenerateDocument(statusChange, ct));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generated PDF for ContextId {ContextId}", statusChange.ContextId);
            }
        }

        await _documentRepository.CreateRange(documents);
        await transaction.CommitAsync(ct);

        return statusChanges.Count == _documentGeneratorConfig.DocumentInsertBatchSize;
    }

    private async Task<DocumentEntity> GenerateDocument(EVotingStatusChangeEntity statusChange, CancellationToken ct)
    {
        _logger.LogDebug("Generating PDF for ContextId {ContextId}...", statusChange.ContextId);
        var pdfStream = statusChange.EVotingRegistered
            ? await _documatrixService.RenderRegisteredPdf(statusChange.Person!, ct)
            : await _documatrixService.RenderUnregisteredPdf(statusChange.Person!, ct);
        _logger.LogDebug("Successfully generated PDF for ContextId {ContextId}", statusChange.ContextId);

        await using var ms = new MemoryStream();
        await pdfStream.CopyToAsync(ms, ct);

        var timestamp = _clock.UtcNow;
        return new DocumentEntity
        {
            CreatedAt = timestamp,
            FileName = BuildFileName(statusChange, timestamp),
            Document = ms.ToArray(),
            StatusChangeId = statusChange.Id,
            WorkerName = _machineConfig.Name,
        };
    }

    private string BuildFileName(EVotingStatusChangeEntity statusChange, DateTime timestamp)
    {
        var statusText = statusChange.EVotingRegistered
            ? RegisteredFileNamePart
            : UnregisteredFileNamePart;

        return $"{timestamp:yyyy-MM-dd}-{statusText}-{statusChange.ContextId}.pdf";
    }
}
