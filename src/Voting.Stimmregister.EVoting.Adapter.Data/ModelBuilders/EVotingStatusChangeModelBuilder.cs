// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Adapter.Data.ModelBuilders;

/// <summary>
/// Model builder for the <see cref="EVotingStatusChangeEntity"/>.
/// </summary>
public class EVotingStatusChangeModelBuilder : IEntityTypeConfiguration<EVotingStatusChangeEntity>
{
    public void Configure(EntityTypeBuilder<EVotingStatusChangeEntity> builder)
    {
        builder.HasOne(x => x.Person)
            .WithOne(x => x.StatusChange)
            .HasForeignKey<PersonEntity>(x => x.StatusChangeId)
            .IsRequired();

        builder.HasOne(x => x.Document)
            .WithOne(x => x.StatusChange)
            .HasForeignKey<DocumentEntity>(x => x.StatusChangeId)
            .IsRequired();

        builder.HasIndex(x => x.ContextId).IsUnique();
        builder.Property(x => x.CreatedAt).HasUtcConversion();
    }
}
