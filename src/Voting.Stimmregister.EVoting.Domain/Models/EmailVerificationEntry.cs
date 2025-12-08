// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Database.Models;

namespace Voting.Stimmregister.EVoting.Domain.Models;

/// <summary>
/// This entity is a short lived entity, which gets deleted when the email is verified.
/// </summary>
public class EmailVerificationEntry : BaseEntity
{
    public DateTime CreatedAt { get; set; }

    public string ContextId { get; set; } = string.Empty;

    public string VerificationCode { get; set; } = string.Empty;

    public long Ahvn13 { get; set; }

    public short BfsCanton { get; set; }

    public DateOnly DateOfBirth { get; set; }

    public string Email { get; set; } = string.Empty;

    public bool IsEmailChange { get; set; }

    public PersonIdentification ToPersonIdentification()
    {
        return new PersonIdentification(Lib.Common.Ahvn13.Parse(Ahvn13), BfsCanton, DateOfBirth, Email);
    }

    public bool IsExpired(DateTime now, TimeSpan validityPeriod)
    {
        return CreatedAt.Add(validityPeriod) < now;
    }
}
