// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Database.Models;

namespace Voting.Stimmregister.EVoting.Domain.Models;

public class DocumentEntity : BaseEntity
{
    public byte[]? Document { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string WorkerName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Guid StatusChangeId { get; set; }

    public EVotingStatusChangeEntity? StatusChange { get; set; }
}
