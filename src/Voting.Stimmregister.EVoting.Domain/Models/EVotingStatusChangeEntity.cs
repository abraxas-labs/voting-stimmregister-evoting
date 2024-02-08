// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Database.Models;

namespace Voting.Stimmregister.EVoting.Domain.Models;

/// <summary>
/// This entity is a short lived entity, which gets deleted with all its child entities,
/// when the document is generated and sent to the recipient.
/// </summary>
public class EVotingStatusChangeEntity : BaseEntity
{
    public bool EVotingRegistered { get; set; }

    public DateTime CreatedAt { get; set; }

    public string ContextId { get; set; } = string.Empty;

    // Note: Persons are not unique. Multiple entries may exists for the same AHVN13 number.
    // A person is a snapshot of the data at the moment that the status change has been created.
    public PersonEntity? Person { get; set; }

    public DocumentEntity? Document { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this status change is active.
    /// If a status change is not active, it has been processed or it was superseded by another status change.
    /// </summary>
    public bool Active { get; set; }
}
