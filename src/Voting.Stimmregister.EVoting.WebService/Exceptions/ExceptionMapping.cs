// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Voting.Lib.Iam.Exceptions;

namespace Voting.Stimmregister.WebService.Exceptions;

internal readonly struct ExceptionMapping
{
    private readonly int _httpStatusCode;

    public ExceptionMapping(int httpStatusCode)
    {
        _httpStatusCode = httpStatusCode;
    }

    public static int MapToHttpStatusCode(Exception ex)
        => Map(ex)._httpStatusCode;

    private static ExceptionMapping Map(Exception ex)
        => ex switch
        {
            NotAuthenticatedException _ => new ExceptionMapping(StatusCodes.Status401Unauthorized),
            ForbiddenException _ => new ExceptionMapping(StatusCodes.Status403Forbidden),
            FluentValidation.ValidationException _ => new ExceptionMapping(StatusCodes.Status400BadRequest),
            ValidationException _ => new ExceptionMapping(StatusCodes.Status400BadRequest),
            _ => new ExceptionMapping(StatusCodes.Status500InternalServerError),
        };
}
