// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Stimmregister.EVoting.Domain.Enums;

namespace Voting.Stimmregister.EVoting.Domain.Exceptions;

public class EVotingNotEnabledException : Exception
{
    public EVotingNotEnabledException(string message, ProcessStatusCode statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public ProcessStatusCode StatusCode { get; }
}
