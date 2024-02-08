// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Data.Repositories;
using Voting.Stimmregister.EVoting.Domain.Diagnostics;

namespace Voting.Stimmregister.EVoting.Core.Services;

public class MetricsWorker
{
    private readonly IEVotingStatusChangeRepository _statusChangeRepository;
    private readonly IClock _clock;

    public MetricsWorker(
        IEVotingStatusChangeRepository statusChangeRepository,
        IClock clock)
    {
        _statusChangeRepository = statusChangeRepository;
        _clock = clock;
    }

    public async Task Run(CancellationToken ct)
    {
        await ReportMetrics(ct);
    }

    private async Task ReportMetrics(CancellationToken ct)
    {
        var countOfActiveStatusChanges = await _statusChangeRepository
            .Query()
            .Where(x => x.Active)
            .CountAsync(ct);

        DiagnosticsConfig.SetActiveStatusChanges(countOfActiveStatusChanges);

        if (countOfActiveStatusChanges == 0)
        {
            DiagnosticsConfig.SetOldestActiveStatusChangeAge(0);
            return;
        }

        var oldestCreationDate = await _statusChangeRepository
            .Query()
            .Where(x => x.Active)
            .MinAsync(x => (DateTime?)x.CreatedAt, ct);

        var ageInHours = oldestCreationDate == null
            ? 0
            : (_clock.UtcNow - oldestCreationDate.Value).Hours;
        DiagnosticsConfig.SetOldestActiveStatusChangeAge(ageInHours);
    }
}
