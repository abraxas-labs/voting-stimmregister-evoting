// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Adapter.Data.ModelBuilders;

/// <summary>
/// Model builder for the <see cref="EmailVerificationEntry"/>.
/// </summary>
public class EmailVerificationModelBuilder : IEntityTypeConfiguration<EmailVerificationEntry>
{
    public void Configure(EntityTypeBuilder<EmailVerificationEntry> builder)
    {
        // Note: The email does not have to be unique, as multiple persons could share the same email (think a family email)
        builder.HasIndex(x => x.Ahvn13).IsUnique();
        builder.HasIndex(x => x.ContextId).IsUnique();
        builder.Property(x => x.CreatedAt).HasUtcConversion();
    }
}
