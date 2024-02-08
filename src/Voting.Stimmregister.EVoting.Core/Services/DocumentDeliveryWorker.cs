// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Voting.Lib.Common;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Data.DataContexts;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Data.Repositories;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.DokConnector;
using Voting.Stimmregister.EVoting.Domain.Diagnostics;
using Voting.Stimmregister.EVoting.Domain.Enums;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Core.Services;

public class DocumentDeliveryWorker
{
    private readonly IDokConnectorService _connectorService;
    private readonly IEVotingStatusChangeRepository _statusChangeRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IDataContext _dataContext;
    private readonly ILogger<DocumentDeliveryWorker> _logger;
    private readonly IClock _clock;

    public DocumentDeliveryWorker(
        IDokConnectorService connectorService,
        IEVotingStatusChangeRepository statusChangeRepository,
        IDocumentRepository documentRepository,
        IDataContext dataContext,
        ILogger<DocumentDeliveryWorker> logger,
        IClock clock)
    {
        _connectorService = connectorService;
        _statusChangeRepository = statusChangeRepository;
        _documentRepository = documentRepository;
        _dataContext = dataContext;
        _logger = logger;
        _clock = clock;
    }

    public async Task Run(CancellationToken ct)
    {
        // Only deliver documents that were generated until this job starts.
        // Otherwise the job may not finish or take longer than expected.
        var now = _clock.UtcNow;

        while (!ct.IsCancellationRequested && await DeliverDocuments(now, ct))
        {
            // generate documents (see statements in while loop)
        }
    }

    private async Task<bool> DeliverDocuments(DateTime maxDocumentDate, CancellationToken ct)
    {
        await using var transaction = await _dataContext.BeginTransaction();
        var statusChange = await _statusChangeRepository.GetNextForDelivery(maxDocumentDate);

        if (statusChange == null)
        {
            return false;
        }

        await UploadDocument(statusChange, ct);

        // Mark the status change as inactive, as it has been processed successfully
        // Also delete the document to free up space in the database
        statusChange.Active = false;
        await _statusChangeRepository.Update(statusChange);
        await _documentRepository.DeleteByKey(statusChange.Document!.Id);

        await transaction.CommitAsync(ct);

        DiagnosticsConfig.IncreaseDeliveredDocumentsCount(
            statusChange.EVotingRegistered ? EVotingStatus.Registered : EVotingStatus.Unregistered,
            statusChange.Person?.MunicipalityBfs ?? 0);

        return true;
    }

    private async Task UploadDocument(EVotingStatusChangeEntity statusChange, CancellationToken ct)
    {
        _logger.LogDebug("Uploading document for ContextId {ContextId}...", statusChange.ContextId);
        using var documentStream = new MemoryStream(statusChange.Document!.Document!);
        await _connectorService.Upload(statusChange.Document.FileName, documentStream, ct);
        _logger.LogDebug("Successfully uploaded document for ContextId {ContextId}.", statusChange.ContextId);
    }
}
