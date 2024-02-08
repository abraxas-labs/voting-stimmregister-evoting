// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Adapter.Data.ModelBuilders;

/// <summary>
/// Model builder for the <see cref="DocumentEntity"/>.
/// </summary>
public class DocumentModelBuilder : IEntityTypeConfiguration<DocumentEntity>
{
    public void Configure(EntityTypeBuilder<DocumentEntity> builder)
    {
        builder
            .Property(x => x.Document)
            .IsRequired();
    }
}
