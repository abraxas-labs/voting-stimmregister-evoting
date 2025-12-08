// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Lib.UserNotifications;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Data.DataContexts;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Data.Repositories;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Stimmregister;
using Voting.Stimmregister.EVoting.Abstractions.Core.Services;
using Voting.Stimmregister.EVoting.Core.Mappings;
using Voting.Stimmregister.EVoting.Core.Utils;
using Voting.Stimmregister.EVoting.Domain.Configuration;
using Voting.Stimmregister.EVoting.Domain.Diagnostics;
using Voting.Stimmregister.EVoting.Domain.Enums;
using Voting.Stimmregister.EVoting.Domain.Exceptions;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Core.Services;

public class EVoterService : IEVoterService
{
    private const string SwissNationality = "Schweiz";

    private readonly EVotingCustomConfig _cantonConfig;
    private readonly short _cantonBfs;
    private readonly EVotingConfig _eVotingConfig;
    private readonly IDataContext _dataContext;
    private readonly ITracingService _tracingService;
    private readonly IClock _clock;
    private readonly IStimmregisterService _stimmregisterService;
    private readonly IEVotingStatusChangeRepository _statusChangeRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IRateLimitService _rateLimitService;
    private readonly IEmailVerificationRepository _emailVerificationRepository;
    private readonly IEmailService _emailService;

    public EVoterService(
        EVotingCustomConfig cantonConfig,
        short cantonBfs,
        EVotingConfig eVotingConfig,
        IStimmregisterService stimmregisterService,
        IClock clock,
        IEVotingStatusChangeRepository statusChangeRepository,
        IDocumentRepository documentRepository,
        ITracingService tracingService,
        IRateLimitService rateLimitService,
        IDataContext dataContext,
        IEmailVerificationRepository emailVerificationRepository,
        IEmailService emailService)
    {
        _cantonConfig = cantonConfig;
        _cantonBfs = cantonBfs;
        _eVotingConfig = eVotingConfig;
        _stimmregisterService = stimmregisterService;
        _clock = clock;
        _statusChangeRepository = statusChangeRepository;
        _documentRepository = documentRepository;
        _tracingService = tracingService;
        _rateLimitService = rateLimitService;
        _emailVerificationRepository = emailVerificationRepository;
        _emailService = emailService;
        _dataContext = dataContext;
    }

    public bool EmailRequired => _cantonConfig.RequiresEmail;

    /// <inheritdoc />
    public async Task<EVotingStatusModel> GetEVotingStatus(PersonIdentification personIdentification, CancellationToken ct)
    {
        await _rateLimitService.CheckAndIncreaseRateLimit(personIdentification.Ahvn13, ct);

        var pendingStatus = await GetPendingStatus(personIdentification);
        if (pendingStatus != null)
        {
            return pendingStatus;
        }

        var stimmregisterInfo = await FetchStimmregisterInfo(personIdentification, ct);
        EnsureEVotingIsEnabledForMunicipality(stimmregisterInfo.Person.MunicipalityBfs);

        return BuildEVotingStatus(stimmregisterInfo);
    }

    /// <inheritdoc />
    public async Task<ProcessStatusCode> Register(PersonIdentification personIdentification, CancellationToken ct)
    {
        await _rateLimitService.CheckAndIncreaseRateLimit(personIdentification.Ahvn13, ct);
        await DeletePendingStatusChange(personIdentification, ct);

        var stimmregisterInfo = await FetchStimmregisterInfo(personIdentification, ct);
        EnsureCanRegister(stimmregisterInfo, personIdentification.BfsCanton);

        var savedPendingStatusChange = await SavePendingStatusChange(personIdentification, ct);
        if (savedPendingStatusChange)
        {
            return ProcessStatusCode.SuccessWithPendingEmailVerification;
        }

        await RegisterInStimmregister(personIdentification, stimmregisterInfo, ct);
        return ProcessStatusCode.Success;
    }

    /// <inheritdoc />
    public async Task Unregister(PersonIdentification personIdentification, CancellationToken ct)
    {
        await _rateLimitService.CheckAndIncreaseRateLimit(personIdentification.Ahvn13, ct);

        var pendingStatusChange = await GetPendingStatus(personIdentification);
        await DeletePendingStatusChange(personIdentification, ct);
        var stimmregisterInfo = await FetchStimmregisterInfo(personIdentification, ct);

        if (stimmregisterInfo.Status is EVotingStatus.Unregistered or EVotingStatus.Unknown)
        {
            if (pendingStatusChange != null)
            {
                // Unregistration deleted the pending status change (e.g. email verification)
                return;
            }

            throw new EVotingValidationException(
                "Die E-Voting Abmeldung kann nicht durchgeführt werden, wenn keine Anmeldung stattgefunden hat.",
                ProcessStatusCode.EVotingAlreadyUnregisteredOrUnknown);
        }

        EnsureEVotingIsEnabledForMunicipality(stimmregisterInfo.Person.MunicipalityBfs);

        if (GetVotingRight(stimmregisterInfo.Person) != VotingRight.Permitted)
        {
            throw new EVotingNotPermittedException("Die E-Voting Abmeldung kann aufgrund fehlender Rechte nicht ausgeführt werden.", ProcessStatusCode.EVotingPermissionError);
        }

        await UnregisterInStimmregister(personIdentification, stimmregisterInfo, ct);
    }

    /// <inheritdoc />
    public async Task ChangeEmail(PersonIdentification personIdentification, CancellationToken ct)
    {
        await _rateLimitService.CheckAndIncreaseRateLimit(personIdentification.Ahvn13, ct);

        if (!EmailRequired)
        {
            throw new EVotingValidationException(
                "Die Email Adresse kann nicht geändert werden, da keine verlangt wird",
                ProcessStatusCode.EmailCannotBeChanged);
        }

        await _rateLimitService.CheckAndIncreaseEmailChangeRateLimit(personIdentification.Ahvn13, ct);
        var pendingVerification = await _emailVerificationRepository.FindValidByAhvn13(
            personIdentification.Ahvn13,
            _cantonConfig.EmailVerificationValidity);
        if (pendingVerification != null)
        {
            // The registration or another email change is already pending. Cancel that and replace it with a new one.
            await SavePendingStatusChange(personIdentification, pendingVerification.IsEmailChange, ct);
            return;
        }

        var stimmregisterInfo = await FetchStimmregisterInfo(personIdentification, ct);
        EnsureIsRegistered(stimmregisterInfo);
        await SavePendingStatusChange(personIdentification, true, ct);
    }

    /// <inheritdoc />
    public async Task VerifyEmail(string verificationCode, CancellationToken ct)
    {
        var verification = await _emailVerificationRepository.Query()
            .FirstOrDefaultAsync(x => x.VerificationCode == verificationCode && x.BfsCanton == _cantonBfs, ct);

        if (verification == null)
        {
            throw new EVotingValidationException("Email-Adresse konnte nicht validiert werden.", ProcessStatusCode.EmailVerificationFailed);
        }

        if (verification.IsExpired(_clock.UtcNow, _cantonConfig.EmailVerificationValidity))
        {
            throw new EVotingValidationException("Verifikations-Code ist abgelaufen.", ProcessStatusCode.EmailVerificationValidityExpired);
        }

        await _emailVerificationRepository.DeleteByKey(verification.Id);

        var personIdentification = verification.ToPersonIdentification();
        var stimmregisterInfo = await FetchStimmregisterInfo(personIdentification, ct);

        if (verification.IsEmailChange)
        {
            await StoreChangedEmail(personIdentification, stimmregisterInfo, ct);
            return;
        }

        EnsureCanRegister(stimmregisterInfo, personIdentification.BfsCanton);
        await RegisterInStimmregister(personIdentification, stimmregisterInfo, ct);
    }

    private async Task StoreChangedEmail(PersonIdentification personIdentification, EVotingInformation stimmregisterInfo, CancellationToken ct)
    {
        EnsureIsRegistered(stimmregisterInfo);

        await _stimmregisterService.UnregisterAsync(personIdentification, ct);
        await _stimmregisterService.RegisterAsync(personIdentification, ct);
        await StoreStatusChange(stimmregisterInfo, true, personIdentification.Email);
    }

    private async Task RegisterInStimmregister(PersonIdentification personIdentification, EVotingInformation stimmregisterInfo, CancellationToken ct)
    {
        await _stimmregisterService.RegisterAsync(personIdentification, ct);
        await StoreStatusChange(stimmregisterInfo, true, personIdentification.Email);

        DiagnosticsConfig.SetEVotingRegistrations(
            stimmregisterInfo.MunicipalityStatistic.EVoterTotalCount + 1,
            stimmregisterInfo.CantonStatistic.EVoterTotalCount + 1,
            stimmregisterInfo.Person.MunicipalityBfs,
            personIdentification.BfsCanton);
    }

    private async Task<bool> SavePendingStatusChange(PersonIdentification personIdentification, CancellationToken ct)
    {
        if (!EmailRequired)
        {
            return false;
        }

        await SavePendingStatusChange(personIdentification, false, ct);
        return true;
    }

    private async Task SavePendingStatusChange(
        PersonIdentification personIdentification,
        bool isEmailChange,
        CancellationToken ct)
    {
        var verificationCode = RandomCodeGenerator.CreateRandomAlphanumericString(_cantonConfig.EmailVerificationCodeLength);

        await using var transaction = await _dataContext.BeginTransaction(IsolationLevel.Serializable);

        // Delete all existing (invalid) email verifications, as only one should be valid at all times
        await DeletePendingStatusChange(personIdentification, ct);

        var creationDate = _clock.UtcNow;
        await _emailVerificationRepository.Create(new EmailVerificationEntry
        {
            Ahvn13 = personIdentification.Ahvn13.ToNumber(),
            BfsCanton = personIdentification.BfsCanton,
            DateOfBirth = personIdentification.DateOfBirth,
            Email = personIdentification.Email!,
            ContextId = _tracingService.ContextId!,
            CreatedAt = creationDate,
            VerificationCode = verificationCode,
            IsEmailChange = isEmailChange,
        });

        await SendVerificationEmail(personIdentification, creationDate, verificationCode, isEmailChange, ct);
        await transaction.CommitAsync(ct);
    }

    private async Task<EVotingStatusModel?> GetPendingStatus(PersonIdentification personIdentification)
    {
        if (!EmailRequired)
        {
            // When emails aren't used, pending verifications shouldn't be used at all.
            return null;
        }

        var pendingVerification = await _emailVerificationRepository.FindValidByAhvn13(
            personIdentification.Ahvn13,
            _cantonConfig.EmailVerificationValidity);

        return pendingVerification == null
            ? null
            : GetPendingStatusModel(pendingVerification.Email);
    }

    private async Task DeletePendingStatusChange(PersonIdentification personIdentification, CancellationToken ct)
    {
        await _emailVerificationRepository.Query()
            .Where(x => x.Ahvn13 == personIdentification.Ahvn13.ToNumber())
            .ExecuteDeleteAsync(ct);
    }

    private async Task<EVotingInformation> FetchStimmregisterInfo(PersonIdentification personIdentification, CancellationToken ct)
    {
        var stimmregisterInfo = await _stimmregisterService.GetEVotingInformationAsync(personIdentification, ct);
        EnsureDateOfBirthMatches(personIdentification, stimmregisterInfo.Person);
        return stimmregisterInfo;
    }

    private void EnsureCanRegister(EVotingInformation stimmregisterInfo, short cantonBfs)
    {
        if (stimmregisterInfo.Status == EVotingStatus.Registered)
        {
            throw new EVotingValidationException(
                "Die eVoting Anmeldung kann nicht durchgeführt werden, wenn die Person bereits angemeldet ist.",
                ProcessStatusCode.EVotingAlreadyRegistered);
        }

        var municipalityBfs = stimmregisterInfo.Person.MunicipalityBfs;
        EnsureEVotingIsEnabledForMunicipality(municipalityBfs);

        // Verify maximum allowed voters on cantonal level
        if (HasReachedMaxAllowedVoters(cantonBfs, stimmregisterInfo.CantonStatistic))
        {
            throw new EVotingValidationException(
                $"Die maximale Anzahl berechtigter eVoting Registrationen für den Kanton mit BFS {cantonBfs} wurde erreicht.",
                ProcessStatusCode.EVotingReachedMaxAllowedVoters);
        }

        // Verify maximum allowed voters on municipal level
        if (HasReachedMaxAllowedVoters(municipalityBfs, stimmregisterInfo.MunicipalityStatistic))
        {
            throw new EVotingValidationException(
                $"Die maximale Anzahl berechtigter eVoting Registrationen für die Gemeinde mit BFS {municipalityBfs} wurde erreicht.",
                ProcessStatusCode.EVotingReachedMaxAllowedVoters);
        }

        if (GetVotingRight(stimmregisterInfo.Person) != VotingRight.Permitted)
        {
            throw new EVotingNotPermittedException("Die eVoting Registration kann aufgrund fehlender Rechte nicht ausgeführt werden.", ProcessStatusCode.EVotingPermissionError);
        }
    }

    private void EnsureIsRegistered(EVotingInformation stimmregisterInfo)
    {
        if (stimmregisterInfo.Status != EVotingStatus.Registered)
        {
            throw new EVotingValidationException(
                "Die Person ist nicht registriert.",
                ProcessStatusCode.EVotingAlreadyUnregisteredOrUnknown);
        }
    }

    private async Task UnregisterInStimmregister(PersonIdentification personIdentification, EVotingInformation stimmregisterInfo, CancellationToken ct)
    {
        await _stimmregisterService.UnregisterAsync(personIdentification, ct);
        await StoreStatusChange(stimmregisterInfo, false);

        DiagnosticsConfig.SetEVotingRegistrations(
            stimmregisterInfo.MunicipalityStatistic.EVoterTotalCount - 1,
            stimmregisterInfo.CantonStatistic.EVoterTotalCount - 1,
            stimmregisterInfo.Person.MunicipalityBfs,
            personIdentification.BfsCanton);
    }

    private EVotingStatusModel BuildEVotingStatus(EVotingInformation eVotingInfo)
    {
        var status = new EVotingStatusModel
        {
            Status = eVotingInfo.Status,
            Right = VotingRight.Permitted,
            Email = eVotingInfo.Person.Email,
        };

        if (!IsPermittedForVoting(eVotingInfo.Person))
        {
            status.ProcessStatusMessage += "Die Person ist nicht berechtigt für E-Voting. ";
            status.Right = VotingRight.NotPermitted;
        }

        if (!IsAbove18Years(eVotingInfo.Person))
        {
            status.ProcessStatusMessage += "Die Person ist zu jung für E-Voting. ";
            status.Right = VotingRight.NotPermitted;
        }

        if (!HasSwissNationality(eVotingInfo.Person))
        {
            status.ProcessStatusMessage += "Die Person verfügt nicht über die Schweizer Nationalität. ";
            status.Right = VotingRight.NotPermitted;
        }

        if (string.IsNullOrEmpty(status.ProcessStatusMessage))
        {
            status.ProcessStatusMessage = "Die E-Voting Statusabfrage war erfolgreich.";
        }

        return status;
    }

    private async Task StoreStatusChange(EVotingInformation stimmregisterInfo, bool registered, string? email = null)
    {
        var statusChange = EVotingMapper.MapToStatusChange(stimmregisterInfo, _tracingService.ContextId!, _clock.UtcNow, email);
        statusChange.EVotingRegistered = registered;

        // Theoretically, it would be possible for a user to register and immediately unregister again.
        // This would cause two writes to happen at the same time and could lead to data problems.
        // An isolation level of serializable prevents this situation.
        await using var transaction = await _dataContext.BeginTransaction(IsolationLevel.Serializable);

        // If an active status change already exists, then the old one should be ignored.
        // If one of them is an unregister status change and the other one a register, we can ignore both of them,
        // since they cancel each other out.
        // Note that both could be a register event if a user registered and shortly after changed his email.
        var existingStatusChange = await _statusChangeRepository.GetActiveByAhvn13(stimmregisterInfo.Person.Ahvn13!.ToNumber());
        statusChange.Active = existingStatusChange == null || statusChange.EVotingRegistered == existingStatusChange.EVotingRegistered;

        if (existingStatusChange != null)
        {
            existingStatusChange.Active = false;
            await DeleteDocumentIfExists(existingStatusChange.Id);
            await _statusChangeRepository.Update(existingStatusChange);
        }

        await _statusChangeRepository.Create(statusChange);
        await transaction.CommitAsync();
    }

    private void EnsureEVotingIsEnabledForMunicipality(short bfsMunicipality)
    {
        if (!_cantonConfig.EVotingEnabledMunicipalitiesList.Contains(bfsMunicipality))
        {
            throw new EVotingNotEnabledException(
                $"Die Gemeinde mit der BFS Nummer {bfsMunicipality} ist nicht für eVoting zugelassen.",
                ProcessStatusCode.EVotingNotEnabledError);
        }
    }

    private void EnsureDateOfBirthMatches(PersonIdentification personIdentification, Person person)
    {
        if (personIdentification.DateOfBirth != person.DateOfBirth)
        {
            throw new EVotingValidationException("The date of birth does not match", ProcessStatusCode.DateOfBirthDoesNotMatch);
        }
    }

    private VotingRight GetVotingRight(Person person) =>
        IsPermittedForVoting(person) && IsAbove18Years(person) && HasSwissNationality(person) ? VotingRight.Permitted : VotingRight.NotPermitted;

    private bool IsPermittedForVoting(Person person) => person.AllowedToVote;

    private bool IsAbove18Years(Person person)
        => person.DateOfBirth < _clock.Today && AgeUtil.CalculateAge(person.DateOfBirth, _clock.Today) >= 18;

    private bool HasSwissNationality(Person person) => person.Nationality?.Equals(SwissNationality, StringComparison.OrdinalIgnoreCase) == true;

    private bool HasReachedMaxAllowedVoters(short bfs, BfsStatistic bfsStatistic)
    {
        var permittedVoterCount = bfsStatistic.VoterTotalCount * _cantonConfig.MaxAllowedVotersPercent / 100;
        var hasReachedLimit = bfsStatistic.EVoterTotalCount >= permittedVoterCount;
        var triggerAlert = bfsStatistic.EVoterTotalCount >= (permittedVoterCount - _eVotingConfig.AlertRegistrationLimitEVoterOffset);

        if (triggerAlert)
        {
            DiagnosticsConfig.SetEVotingReachedMaxAllowedEVoters(
                bfs,
                bfsStatistic.EVoterTotalCount,
                bfsStatistic.VoterTotalCount,
                _cantonConfig.MaxAllowedVotersPercent,
                permittedVoterCount);
        }

        return hasReachedLimit;
    }

    private EVotingStatusModel GetPendingStatusModel(string? email)
    {
        return new EVotingStatusModel
        {
            Status = EVotingStatus.PendingEmailVerification,
            Right = VotingRight.Permitted,
            Email = email,
            ProcessStatusMessage =
                "Die Email-Adresse muss noch bestätigt werden, bevor die Anmeldung definitiv erfolgt.",
        };
    }

    private async Task DeleteDocumentIfExists(Guid statusChangeId)
    {
        var existingDocument = await _documentRepository
            .Query()
            .FirstOrDefaultAsync(d => d.StatusChangeId == statusChangeId);
        if (existingDocument != null)
        {
            await _documentRepository.DeleteByKey(existingDocument.Id);
        }
    }

    private async Task SendVerificationEmail(
        PersonIdentification personIdentification,
        DateTime creationDate,
        string verificationCode,
        bool isEmailChange,
        CancellationToken ct)
    {
        var subject = "Anmeldung E-Voting: Verifizierung E-Mail-Adresse";
        var code = WebUtility.UrlEncode(verificationCode);
        var builder = new UriBuilder(_cantonConfig.EmailVerificationCallbackUrl)
        {
            Fragment = $"code={code}&cantonBfs={personIdentification.BfsCanton}",
        };
        var url = builder.Uri.ToString();
        var message = isEmailChange
            ? $"Sie haben am {creationDate:dd.MM.yyyy} um {creationDate:HH:mm:ss} Uhr Ihre E-Mail-Adresse für E-Voting im Kanton {_cantonConfig.CantonName} geändert."
            : $"Sie haben sich am {creationDate:dd.MM.yyyy} um {creationDate:HH:mm:ss} Uhr für E-Voting im Kanton {_cantonConfig.CantonName} angemeldet.";
        var body = Html(
            $$"""
              <!DOCTYPE html>
              <html lang="de-CH">
              <head>
                <meta charset="UTF-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
                <title>{{subject}}</title>
                <style>
                  body {
                    font-family: Arial, sans-serif;
                    background-color: #f4f4f4;
                    margin: 0;
                    padding: 0;
                  }
                  .container {
                    background-color: #ffffff;
                    max-width: 600px;
                    margin: 40px auto;
                    padding: 20px;
                    border-radius: 6px;
                    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
                  }
                </style>
              </head>
              <body>
                <div class="container">
                    {{message}}
                    Bitte verifizieren Sie Ihre E-Mail-Adresse mit folgendem Link, um den Prozess abzuschliessen:
                    <a href="{{url}}">{{url}}</a>
                </div>
              </body>
              </html>
              """);

        await _emailService.Send(_cantonConfig.Smtp, new UserNotification(personIdentification.Email!, subject, body), ct);
    }

    private string Html([StringSyntax("html")] string html) => html;
}
