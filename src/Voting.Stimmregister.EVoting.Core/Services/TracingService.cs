// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Http;
using Voting.Stimmregister.EVoting.Abstractions.Core.Services;

namespace Voting.Stimmregister.EVoting.Core.Services;

public class TracingService : ITracingService
{
    private const string XContextIdHeaderKey = "X-Context-Id";
    private string? _contextId;

    public string? ContextId => _contextId;

    public string? GetAndStoreContextId(HttpContext httpContext)
    {
        return _contextId = httpContext.Request.Headers.TryGetValue(XContextIdHeaderKey, out var id)
            ? id.ToString()
            : null;
    }
}
