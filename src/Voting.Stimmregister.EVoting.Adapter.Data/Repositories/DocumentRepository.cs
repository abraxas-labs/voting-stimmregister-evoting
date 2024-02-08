// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Database.Repositories;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Data.Repositories;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Adapter.Data.Repositories;

public class DocumentRepository : DbRepository<DataContext, DocumentEntity>, IDocumentRepository
{
    public DocumentRepository(DataContext context)
        : base(context)
    {
    }
}
