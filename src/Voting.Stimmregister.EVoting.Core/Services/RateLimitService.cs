// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Data;
using System.Linq;
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
        var actionCount = await _rateLimitRepository.Query()
            .Where(x => x.Ahvn13 == ahvn13.ToNumber() && x.Date == today)
            .Select(x => x.ActionCount)
            .FirstOrDefaultAsync(ct);

        if (actionCount >= _config.RateLimitPerAhvn13PerDay)
        {
            throw new EVotingValidationException("Rate limit exceeded", ProcessStatusCode.RateLimitExceeded);
        }

        var rateLimit = await IncreaseActionCount(ahvn13, actionDelta: 1, ct: ct);
        await transaction.CommitAsync(ct);
        DiagnosticsConfig.SetRateLimit(rateLimit.ActionCount, rateLimit.Date.ToString("yyyy-MM-dd"), rateLimit.Id.ToString());
    }

    public async Task CheckAndIncreaseEmailChangeRateLimit(Ahvn13 ahvn13, CancellationToken ct)
    {
        var today = _clock.Today;
        var lastWeek = today.AddDays(-7);

        await using var transaction = await _dataContext.BeginTransaction(IsolationLevel.RepeatableRead);

        var emailChangeToday = await _rateLimitRepository.Query()
            .Where(x => x.Ahvn13 == ahvn13.ToNumber() && x.Date == today)
            .Select(x => x.EmailChangeCount)
            .FirstOrDefaultAsync(ct);
        if (emailChangeToday >= _config.EmailChangeLimitPerAhvn13PerDay)
        {
            throw new EVotingValidationException("Email change rate limit per day exceeded", ProcessStatusCode.EmailChangeRateLimitExceeded);
        }

        var emailChangePastWeek = await _rateLimitRepository.Query()
            .Where(x => x.Ahvn13 == ahvn13.ToNumber() && x.Date > lastWeek && x.Date <= today)
            .SumAsync(x => x.EmailChangeCount, ct);
        if (emailChangePastWeek >= _config.EmailChangeLimitPerAhvn13PerWeek)
        {
            throw new EVotingValidationException("Email change rate limit per week exceeded", ProcessStatusCode.EmailChangeRateLimitExceeded);
        }

        await IncreaseActionCount(ahvn13, emailChangeDelta: 1, ct: ct);
        await transaction.CommitAsync(ct);
    }

    private async Task<RateLimitEntity> IncreaseActionCount(Ahvn13 ahvn13, int actionDelta = 0, int emailChangeDelta = 0, CancellationToken ct = default)
    {
        var today = _clock.Today;
        var rateLimit = await _rateLimitRepository.Query()
            .FirstOrDefaultAsync(x => x.Ahvn13 == ahvn13.ToNumber() && x.Date == today, ct);

        if (rateLimit == null)
        {
            rateLimit = new RateLimitEntity
            {
                Ahvn13 = ahvn13.ToNumber(),
                Date = today,
                ActionCount = actionDelta,
                EmailChangeCount = emailChangeDelta,
            };
            await _rateLimitRepository.Create(rateLimit);
        }
        else
        {
            rateLimit.ActionCount += actionDelta;
            rateLimit.EmailChangeCount += emailChangeDelta;
            await _rateLimitRepository.Update(rateLimit);
        }

        return rateLimit;
    }
}
