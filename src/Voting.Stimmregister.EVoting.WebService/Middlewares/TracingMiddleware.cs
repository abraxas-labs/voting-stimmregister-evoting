// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog.Context;
using Voting.Stimmregister.EVoting.Abstractions.Core.Services;

namespace Voting.Stimmregister.EVoting.WebService.Middlewares;

public class TracingMiddleware
{
    private readonly RequestDelegate _next;

    public TracingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ITracingService tracingService)
    {
        var contextId = tracingService.GetAndStoreContextId(context);
        if (!string.IsNullOrEmpty(contextId))
        {
            LogContext.PushProperty("ContextId", contextId);
        }

        await _next.Invoke(context);
    }
}
