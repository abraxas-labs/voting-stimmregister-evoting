// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Voting.Stimmregister.EVoting.Abstractions.Core.Services;
using Voting.Stimmregister.EVoting.Domain.Enums;
using Voting.Stimmregister.EVoting.Domain.Exceptions;

namespace Voting.Stimmregister.EVoting.Rest.Attributes;

public class ContextRequiredAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var tracingService = context.HttpContext.RequestServices.GetRequiredService<ITracingService>();

        if (string.IsNullOrWhiteSpace(tracingService.ContextId))
        {
            throw new EVotingValidationException("Die Context-ID muss gesetzt sein", ProcessStatusCode.ContextNotProvided);
        }
    }
}
