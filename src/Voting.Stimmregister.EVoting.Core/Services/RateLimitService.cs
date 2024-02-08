// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Data.DataContexts;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Data.Repositories;
using Voting.Stimmregister.EVoting.Abstractions.Core.Services;
using Voting.Stimmregister.EVoting.Domain.Configuration;
using Voting.Stimmregister.EVoting.Domain.Diagnostics;
using Voting.Stimmregister.EVoting.Domain.Enums;
using Voting.Stimmregister.EVoting.Domain.Exceptions;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Core.Services;

public class RateLimitService : IRateLimitService
{
    private readonly IDataContext _dataContext;
    private readonly IRateLimitRepository _rateLimitRepository;
    private readonly RateLimitConfig _config;
    private readonly IClock _clock;

    public RateLimitService(
        IDataContext dataContext,
        IRateLimitRepository rateLimitRepository,
        RateLimitConfig config,
        IClock clock)
    {
        _dataContext = dataContext;
        _rateLimitRepository = rateLimitRepository;
        _config = config;
        _clock = clock;
    }

    public async Task CheckAndIncreaseRateLimit(Ahvn13 ahvn13, CancellationToken ct)
    {
        var today = _clock.Today;

        await using var transaction = await _dataContext.BeginTransaction(IsolationLevel.RepeatableRead);
        var rateLimit = await _rateLimitRepository.Query()
            .FirstOrDefaultAsync(x => x.Ahvn13 == ahvn13.ToNumber() && x.Date == today, ct);

        if (rateLimit == null)
        {
            await _rateLimitRepository.Create(new RateLimitEntity
            {
                Ahvn13 = ahvn13.ToNumber(),
                Date = today,
                ActionCount = 1,
            });
            await transaction.CommitAsync(ct);
            return;
        }

        rateLimit.ActionCount++;
        if (rateLimit.ActionCount > _config.RateLimitPerAhvn13PerDay)
        {
            throw new EVotingValidationException("Rate limit exceeded", ProcessStatusCode.RateLimitExceeded);
        }

        await _rateLimitRepository.Update(rateLimit);
        await transaction.CommitAsync(ct);

        DiagnosticsConfig.SetRateLimit(rateLimit.ActionCount, rateLimit.Date.ToString("yyyy-MM-dd"), rateLimit.Id.ToString());
    }
}
