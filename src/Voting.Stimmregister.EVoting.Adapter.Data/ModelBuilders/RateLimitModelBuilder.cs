// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Adapter.Data.ModelBuilders;

/// <summary>
/// Model builder for the <see cref="RateLimitEntity"/>.
/// </summary>
public class RateLimitModelBuilder : IEntityTypeConfiguration<RateLimitEntity>
{
    public void Configure(EntityTypeBuilder<RateLimitEntity> builder)
    {
        builder.HasIndex(x => new { x.Ahvn13, x.Date }).IsUnique();
    }
}
