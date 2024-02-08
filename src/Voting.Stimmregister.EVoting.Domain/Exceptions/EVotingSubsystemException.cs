// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Stimmregister.EVoting.Domain.Enums;

namespace Voting.Stimmregister.EVoting.Domain.Exceptions;

public class EVotingSubsystemException : Exception
{
    public EVotingSubsystemException(string? message)
    : base(message)
    {
    }

    public EVotingSubsystemException(string? message, ProcessStatusCode statusCode)
    : base(message)
    {
        StatusCode = statusCode;
    }

    public EVotingSubsystemException(string? message, Exception? innerException)
    : base(message, innerException)
    {
    }

    public ProcessStatusCode StatusCode { get; }
}
