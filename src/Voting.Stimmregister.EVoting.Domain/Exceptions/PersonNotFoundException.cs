// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;

namespace Voting.Stimmregister.EVoting.Domain.Exceptions;

public class PersonNotFoundException : Exception
{
    public PersonNotFoundException(object id)
        : base($"Entity with id {id} not found")
    {
    }

    public PersonNotFoundException(string type, object id)
       : base($"{type} with id {id} not found")
    {
    }

    public PersonNotFoundException(Type type, object id)
       : base($"{type} with id {id} not found")
    {
    }

    public PersonNotFoundException(string? message, Exception? innerException)
    : base(message, innerException)
    {
    }
}
