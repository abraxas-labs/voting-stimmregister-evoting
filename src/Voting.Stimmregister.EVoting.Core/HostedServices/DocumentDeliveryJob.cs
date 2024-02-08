﻿// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading;
using System.Threading.Tasks;
using Voting.Lib.Scheduler;
using Voting.Stimmregister.EVoting.Core.Services;

namespace Voting.Stimmregister.EVoting.Core.HostedServices;

public sealed class DocumentDeliveryJob : IScheduledJob
{
    private readonly DocumentDeliveryWorker _worker;

    public DocumentDeliveryJob(DocumentDeliveryWorker worker)
    {
        _worker = worker;
    }

    public async Task Run(CancellationToken ct)
    {
        await _worker.Run(ct);
    }
}
