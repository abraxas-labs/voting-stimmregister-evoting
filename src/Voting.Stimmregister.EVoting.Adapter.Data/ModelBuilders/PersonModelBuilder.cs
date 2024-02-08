// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Adapter.Data.ModelBuilders;

/// <summary>
/// Model builder for the <see cref="PersonEntity"/>.
/// </summary>
public class PersonModelBuilder : IEntityTypeConfiguration<PersonEntity>
{
    public void Configure(EntityTypeBuilder<PersonEntity> builder)
    {
        builder.OwnsOne(x => x.Address);
        builder.Navigation(x => x.Address).IsRequired();
    }
}
