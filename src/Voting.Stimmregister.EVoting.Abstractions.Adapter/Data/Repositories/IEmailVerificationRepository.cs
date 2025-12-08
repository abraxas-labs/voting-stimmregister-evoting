// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Lib.Database.Repositories;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Abstractions.Adapter.Data.Repositories;

/// <summary>
/// Repository for e-voting status change table.
/// </summary>
public interface IEmailVerificationRepository : IDbRepository<DbContext, EmailVerificationEntry>
{
    Task<EmailVerificationEntry?> FindValidByAhvn13(Ahvn13 ahvn13, TimeSpan validityPeriod);
}
