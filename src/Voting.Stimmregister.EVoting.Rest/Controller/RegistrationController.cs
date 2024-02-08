// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Voting.Lib.Common;
using Voting.Stimmregister.EVoting.Abstractions.Core.Services;
using Voting.Stimmregister.EVoting.Domain.Enums;
using Voting.Stimmregister.EVoting.Domain.Exceptions;
using Voting.Stimmregister.EVoting.Domain.Models;
using Voting.Stimmregister.EVoting.Rest.Attributes;
using Voting.Stimmregister.EVoting.Rest.Models.Request;
using Voting.Stimmregister.EVoting.Rest.Models.Response;

namespace Voting.Stimmregister.EVoting.Rest.Controller;

[Route("v1")]
[AuthorizeAdmin]
public class RegistrationController : ControllerBase
{
    private readonly IRegistrationService _registrationService;
    private readonly IEVoterService _eVoterService;

    public RegistrationController(IRegistrationService registrationService, IEVoterService eVoterService)
    {
        _registrationService = registrationService;
        _eVoterService = eVoterService;
    }

    [HttpPost("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProcessStatusResponseBase), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(nameof(GetRegistrationStatus))]
    public async Task<GetStatusResponse> GetRegistrationStatus([FromBody] GetStatusRequest request)
    {
        var personIdentification = ValidatePersonIdentification(request.Ahvn13, request.BfsCanton, request.DateOfBirth);
        var evotingStatus = await _eVoterService.GetEVotingStatus(personIdentification, HttpContext.RequestAborted);

        return new GetStatusResponse
        {
            ProcessStatusCode = ProcessStatusCode.Success,
            VotingRight = evotingStatus.Right,
            VotingStatus = evotingStatus.Status,
        };
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProcessStatusResponseBase), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(nameof(RegisterForEVoting))]
    [ContextRequired]
    public async Task<RegisterResponse> RegisterForEVoting([FromBody] RegisterRequest request)
    {
        var personIdentification = ValidatePersonIdentification(request.Ahvn13, request.BfsCanton, request.DateOfBirth);
        await _registrationService.Register(personIdentification, HttpContext.RequestAborted);
        return new RegisterResponse
        {
            ProcessStatusCode = ProcessStatusCode.Success,
            ProcessStatusMessage = "Die Anmeldung für eVoting war erfolgreich.",
        };
    }

    [HttpPost("unregister")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProcessStatusResponseBase), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(nameof(UnregisterFromEVoting))]
    [ContextRequired]
    public async Task<UnregisterResponse> UnregisterFromEVoting([FromBody] UnregisterRequest request)
    {
        var personIdentification = ValidatePersonIdentification(request.Ahvn13, request.BfsCanton, request.DateOfBirth);
        await _registrationService.Unregister(personIdentification, HttpContext.RequestAborted);
        return new UnregisterResponse
        {
            ProcessStatusCode = ProcessStatusCode.Success,
            ProcessStatusMessage = "Die Abmeldung für eVoting war erfolgreich.",
        };
    }

    private PersonIdentification ValidatePersonIdentification(string ahvn13, short bfs, DateOnly dateOfBirth)
    {
        var parsedAhvn13 = ValidateAhvn13(ahvn13);
        ValidateBfsCantonNumber(bfs);

        return new PersonIdentification(parsedAhvn13, bfs, dateOfBirth);
    }

    private Ahvn13 ValidateAhvn13(string ahvn13)
    {
        if (!Ahvn13.TryParse(ahvn13, out var parsed))
        {
            throw new EVotingValidationException(
                "Die AHVN13 hat ein ungültiges Format. Erwartetes Format '756.xxxx.xxxx.xc",
                ProcessStatusCode.InvalidAhvn13Format);
        }

        return parsed;
    }

    private void ValidateBfsCantonNumber(short bfs)
    {
        const short min = 1;
        const short max = 26;

        if (bfs < min || bfs > max)
        {
            throw new EVotingValidationException(
                $"Die BFS Kantonsnummer liegt ausserhalb des Gültigkeitsbereiches [{min}...{max}].'",
                ProcessStatusCode.InvalidBfsCantonFormat);
        }
    }
}
