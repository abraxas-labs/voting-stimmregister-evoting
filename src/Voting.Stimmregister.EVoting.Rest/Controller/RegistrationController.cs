// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Voting.Lib.Common;
using Voting.Lib.Validation;
using Voting.Stimmregister.EVoting.Core.Services;
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
    private readonly EVoterServiceFactory _eVoterServiceFactory;

    public RegistrationController(EVoterServiceFactory eVoterServiceFactory)
    {
        _eVoterServiceFactory = eVoterServiceFactory;
    }

    [HttpPost("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProcessStatusResponseBase), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(nameof(GetRegistrationStatus))]
    public async Task<GetStatusResponse> GetRegistrationStatus([FromBody] GetStatusRequest request)
    {
        var personIdentification = ValidatePersonIdentification(request.Ahvn13, request.BfsCanton, request.DateOfBirth, false);
        var eVoterService = _eVoterServiceFactory.CreateEVoterService(request.BfsCanton);
        var evotingStatus = await eVoterService.GetEVotingStatus(personIdentification, HttpContext.RequestAborted);

        return new GetStatusResponse
        {
            ProcessStatusCode = ProcessStatusCode.Success,
            VotingRight = evotingStatus.Right,
            VotingStatus = evotingStatus.Status,
            Email = evotingStatus.Email,
        };
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProcessStatusResponseBase), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(nameof(RegisterForEVoting))]
    [ContextRequired]
    public async Task<RegisterResponse> RegisterForEVoting([FromBody] RegisterRequest request)
    {
        var eVoterService = _eVoterServiceFactory.CreateEVoterService(request.BfsCanton);
        var personIdentification = ValidatePersonIdentification(
            request.Ahvn13,
            request.BfsCanton,
            request.DateOfBirth,
            eVoterService.EmailRequired,
            request.Email);
        var status = await eVoterService.Register(personIdentification, HttpContext.RequestAborted);
        return new RegisterResponse
        {
            ProcessStatusCode = status,
            ProcessStatusMessage = status == ProcessStatusCode.SuccessWithPendingEmailVerification
                ? "Die Anmeldung wird durchgeführt, sobald die Email-Adresse bestätigt wird."
                : "Die Anmeldung für E-Voting war erfolgreich.",
        };
    }

    [HttpPost("unregister")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProcessStatusResponseBase), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(nameof(UnregisterFromEVoting))]
    [ContextRequired]
    public async Task<UnregisterResponse> UnregisterFromEVoting([FromBody] UnregisterRequest request)
    {
        var personIdentification = ValidatePersonIdentification(request.Ahvn13, request.BfsCanton, request.DateOfBirth, false);
        var eVoterService = _eVoterServiceFactory.CreateEVoterService(request.BfsCanton);
        await eVoterService.Unregister(personIdentification, HttpContext.RequestAborted);
        return new UnregisterResponse
        {
            ProcessStatusCode = ProcessStatusCode.Success,
            ProcessStatusMessage = "Die Abmeldung für eVoting war erfolgreich.",
        };
    }

    [HttpPost("change-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProcessStatusResponseBase), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(nameof(ChangeEVotingEmail))]
    [ContextRequired]
    public async Task<ChangeEmailResponse> ChangeEVotingEmail([FromBody] ChangeEmailRequest request)
    {
        var eVoterService = _eVoterServiceFactory.CreateEVoterService(request.BfsCanton);
        var personIdentification = ValidatePersonIdentification(
            request.Ahvn13,
            request.BfsCanton,
            request.DateOfBirth,
            true,
            request.Email);
        await eVoterService.ChangeEmail(personIdentification, HttpContext.RequestAborted);
        return new ChangeEmailResponse
        {
            ProcessStatusCode = ProcessStatusCode.SuccessWithPendingEmailVerification,
            ProcessStatusMessage = "Die Email-Adresse wird geändert, sobald die neue Email-Adresse bestätigt wird.",
        };
    }

    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProcessStatusResponseBase), StatusCodes.Status400BadRequest)]
    [SwaggerOperation(nameof(VerifyEmail))]
    public async Task<VerifyEmailResponse> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        ValidateBfsCantonNumber(request.BfsCanton);

        var eVoterService = _eVoterServiceFactory.CreateEVoterService(request.BfsCanton);
        await eVoterService.VerifyEmail(request.Code, HttpContext.RequestAborted);

        return new VerifyEmailResponse
        {
            ProcessStatusCode = ProcessStatusCode.Success,
            ProcessStatusMessage = "Email erfolgreich verifiziert. Anmeldung wurde durchgeführt.",
        };
    }

    private PersonIdentification ValidatePersonIdentification(
        string ahvn13,
        short bfs,
        DateOnly dateOfBirth,
        bool emailRequired,
        string? email = null)
    {
        var parsedAhvn13 = ValidateAhvn13(ahvn13);
        ValidateBfsCantonNumber(bfs);
        ValidateEmail(email, emailRequired);

        return new PersonIdentification(parsedAhvn13, bfs, dateOfBirth, email);
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

    private void ValidateEmail(string? email, bool required)
    {
        if (!required && string.IsNullOrEmpty(email))
        {
            return;
        }

        if (email == null || !StringValidation.EmailRegex.IsMatch(email))
        {
            throw new EVotingValidationException(
                "Die Email-Adresse hat ein ungültiges Format.",
                ProcessStatusCode.InvalidEmailFormat);
        }
    }
}
