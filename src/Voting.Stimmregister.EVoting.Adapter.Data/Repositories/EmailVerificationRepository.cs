// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Lib.Database.Repositories;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Data.Repositories;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Adapter.Data.Repositories;

/// <inheritdoc cref="IEmailVerificationRepository"/>
public class EmailVerificationRepository : DbRepository<DataContext, EmailVerificationEntry>, IEmailVerificationRepository
{
    private readonly IClock _clock;

    public EmailVerificationRepository(DataContext context, IClock clock)
        : base(context)
    {
        _clock = clock;
    }

    public async Task<EmailVerificationEntry?> FindValidByAhvn13(Ahvn13 ahvn13, TimeSpan validityPeriod)
    {
        var entry = await Query()
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(x => x.Ahvn13 == ahvn13.ToNumber());

        return entry == null || entry.IsExpired(_clock.UtcNow, validityPeriod)
            ? null
            : entry;
    }
}
