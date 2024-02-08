// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Http;

namespace Voting.Stimmregister.EVoting.Abstractions.Core.Services;

public interface ITracingService
{
    string? ContextId { get; }

    string? GetAndStoreContextId(HttpContext httpContext);
}
