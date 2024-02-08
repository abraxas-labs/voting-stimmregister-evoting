// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Voting.Lib.Database.Repositories;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Abstractions.Adapter.Data.Repositories;

/// <summary>
/// Repository for document table.
/// </summary>
public interface IDocumentRepository : IDbRepository<DbContext, DocumentEntity>
{
}
