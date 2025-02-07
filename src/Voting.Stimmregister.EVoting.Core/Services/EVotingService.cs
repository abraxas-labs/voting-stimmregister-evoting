// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Common;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Data.DataContexts;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Data.Repositories;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Stimmregister;
using Voting.Stimmregister.EVoting.Abstractions.Core.Services;
using Voting.Stimmregister.EVoting.Core.Mappings;
using Voting.Stimmregister.EVoting.Domain.Configuration;
using Voting.Stimmregister.EVoting.Domain.Diagnostics;
using Voting.Stimmregister.EVoting.Domain.Enums;
using Voting.Stimmregister.EVoting.Domain.Exceptions;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Core.Services;

public class EVotingService : IRegistrationService, IEVoterService
{
    private const string SwissNationality = "Schweiz";

    private readonly EVotingConfig _eVotingConfig;
    private readonly IStimmregisterService _stimmregisterService;
    private readonly IClock _clock;
    private readonly IEVotingStatusChangeRepository _statusChangeRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ITracingService _tracingService;
    private readonly IRateLimitService _rateLimitService;
    private readonly IDataContext _dataContext;

    public EVotingService(
        EVotingConfig eVotingConfig,
        IStimmregisterService stimmregisterService,
        IClock clock,
        IEVotingStatusChangeRepository statusChangeRepository,
        IDocumentRepository documentRepository,
        ITracingService tracingService,
        IRateLimitService rateLimitService,
        IDataContext dataContext)
    {
        _eVotingConfig = eVotingConfig;
        _stimmregisterService = stimmregisterService;
        _clock = clock;
        _statusChangeRepository = statusChangeRepository;
        _documentRepository = documentRepository;
        _tracingService = tracingService;
        _rateLimitService = rateLimitService;
        _dataContext = dataContext;
    }

    public async Task<EVotingStatusModel> GetEVotingStatus(PersonIdentification personIdentification, CancellationToken ct)
    {
        await _rateLimitService.CheckAndIncreaseRateLimit(personIdentification.Ahvn13, ct);

        var stimmregisterInfo = await _stimmregisterService.GetEVotingInformationAsync(personIdentification, ct);
        EnsureDateOfBirthMatches(personIdentification, stimmregisterInfo.Person);

        EnsureEVotingIsEnabledForMunicipality(personIdentification.BfsCanton, stimmregisterInfo.Person.MunicipalityBfs);

        var eVotingInfo = new EVotingStatusModel
        {
            Status = stimmregisterInfo.Status,
            Right = VotingRight.Permitted,
        };

        if (!IsPermittedForVoting(stimmregisterInfo.Person))
        {
            eVotingInfo.ProcessStatusMessage += "Die Person ist nicht berechtigt für eVoting. ";
            eVotingInfo.Right = VotingRight.NotPermitted;
        }

        if (!IsAbove18Years(stimmregisterInfo.Person))
        {
            eVotingInfo.ProcessStatusMessage += "Die Person ist zu Jung für eVoting. ";
            eVotingInfo.Right = VotingRight.NotPermitted;
        }

        if (!HasSwissNationality(stimmregisterInfo.Person))
        {
            eVotingInfo.ProcessStatusMessage += "Die Person verfügt nicht über die schweizer Nationalität. ";
            eVotingInfo.Right = VotingRight.NotPermitted;
        }

        eVotingInfo.ProcessStatusMessage = string.IsNullOrEmpty(eVotingInfo.ProcessStatusMessage)
            ? "Die eVoting Statusabfrage war erfolgreich."
            : eVotingInfo.ProcessStatusMessage;

        return eVotingInfo;
    }

    public async Task Register(PersonIdentification personIdentification, CancellationToken ct)
    {
        await _rateLimitService.CheckAndIncreaseRateLimit(personIdentification.Ahvn13, ct);

        var stimmregisterInfo = await _stimmregisterService.GetEVotingInformationAsync(personIdentification, ct);
        EnsureDateOfBirthMatches(personIdentification, stimmregisterInfo.Person);

        // Note: VOTING Stimmregister is the source of truth, only check for that status and not in the local database
        if (stimmregisterInfo.Status == EVotingStatus.Registered)
        {
            throw new EVotingValidationException(
                "Die eVoting Anmeldung kann nicht durchgeführt werden wenn die Person bereits angemeldet ist.",
                ProcessStatusCode.EVotingAlreadyRegistered);
        }

        var municipalityBfs = stimmregisterInfo.Person.MunicipalityBfs;
        EnsureEVotingIsEnabledForMunicipality(personIdentification.BfsCanton, municipalityBfs);

        // Verify maximum allowed voters on cantonal level
        if (HasReachedMaxAllowedVoters(personIdentification.BfsCanton, stimmregisterInfo.CantonStatistic))
        {
            throw new EVotingValidationException(
                $"Die maximale Anzahl berechtigter eVoting Registrationen für den Kanton mit BFS {personIdentification.BfsCanton} wurde erreicht.",
                ProcessStatusCode.EVotingReachedMaxAllowedVoters);
        }

        // Verify maximum allowed voters on municipal level
        if (HasReachedMaxAllowedVoters(municipalityBfs, stimmregisterInfo.MunicipalityStatistic, false))
        {
            throw new EVotingValidationException(
                $"Die maximale Anzahl berechtigter eVoting Registrationen für die Gemeinde mit BFS {municipalityBfs} wurde erreicht.",
                ProcessStatusCode.EVotingReachedMaxAllowedVoters);
        }

        if (GetVotingRight(stimmregisterInfo.Person) != VotingRight.Permitted)
        {
            throw new EVotingNotPermittedException("Die eVoting Registration kann aufgrund fehlender Rechte nicht ausgeführt werden.", ProcessStatusCode.EVotingPermissionError);
        }

        await _stimmregisterService.RegisterAsync(personIdentification, ct);
        await StoreStatusChange(stimmregisterInfo, true);

        DiagnosticsConfig.SetEVotingRegistrations(
            stimmregisterInfo.MunicipalityStatistic.EVoterTotalCount + 1,
            stimmregisterInfo.CantonStatistic.EVoterTotalCount + 1,
            stimmregisterInfo.Person.MunicipalityBfs,
            personIdentification.BfsCanton);
    }

    public async Task Unregister(PersonIdentification personIdentification, CancellationToken ct)
    {
        await _rateLimitService.CheckAndIncreaseRateLimit(personIdentification.Ahvn13, ct);

        var stimmregisterInfo = await _stimmregisterService.GetEVotingInformationAsync(personIdentification, ct);
        EnsureDateOfBirthMatches(personIdentification, stimmregisterInfo.Person);

        // Note: VOTING Stimmregister is the source of truth, only check for that status and not in the local database
        if (stimmregisterInfo.Status is EVotingStatus.Unregistered or EVotingStatus.Unknown)
        {
            throw new EVotingValidationException(
                "Die eVoting Abmeldung kann nicht durchgeführt werden wenn keine Anmeldung stattgefunden hat.",
                ProcessStatusCode.EVotingAlreadyUnregisteredOrUnknown);
        }

        EnsureEVotingIsEnabledForMunicipality(personIdentification.BfsCanton, stimmregisterInfo.Person.MunicipalityBfs);

        if (GetVotingRight(stimmregisterInfo.Person) != VotingRight.Permitted)
        {
            throw new EVotingNotPermittedException("Die eVoting Abmeldung kann aufgrund fehlender Rechte nicht ausgeführt werden.", ProcessStatusCode.EVotingPermissionError);
        }

        await _stimmregisterService.UnregisterAsync(personIdentification, ct);
        await StoreStatusChange(stimmregisterInfo, false);

        DiagnosticsConfig.SetEVotingRegistrations(
            stimmregisterInfo.MunicipalityStatistic.EVoterTotalCount - 1,
            stimmregisterInfo.CantonStatistic.EVoterTotalCount - 1,
            stimmregisterInfo.Person.MunicipalityBfs,
            personIdentification.BfsCanton);
    }

    private async Task StoreStatusChange(EVotingInformation stimmregisterInfo, bool registered)
    {
        var statusChange = EVotingMapper.MapToStatusChange(stimmregisterInfo, _tracingService.ContextId!, _clock.UtcNow);
        statusChange.EVotingRegistered = registered;

        // Theoretically, it would be possible for an user to register and immediately unregister again.
        // This would cause two writes to happen at the same time and could lead to data problems.
        // An isolation level of serializable prevents this situation.
        await using var transaction = await _dataContext.BeginTransaction(IsolationLevel.Serializable);

        // If an active status change already exists, this means that one is a register status, the other one an unregister,
        // resulting in the same state as if neither of them happened.
        // We can simply set both of them to inactive, neither of them will be processed.
        var existingStatusChange = await _statusChangeRepository.GetActiveByAhvn13(stimmregisterInfo.Person.Ahvn13!.ToNumber());
        if (existingStatusChange == null)
        {
            statusChange.Active = true;
        }
        else
        {
            statusChange.Active = false;
            existingStatusChange.Active = false;
            await DeleteDocumentIfExists(existingStatusChange.Id);
            await _statusChangeRepository.Update(existingStatusChange);
        }

        await _statusChangeRepository.Create(statusChange);
        await transaction.CommitAsync();
    }

    private void EnsureEVotingIsEnabledForMunicipality(short bfsCanton, short bfsMunicipality)
    {
        if (!_eVotingConfig.CustomSettings.ContainsKey(bfsCanton.ToString()))
        {
            throw new InvalidOperationException(
                $"Für den Kunden mit BFS {bfsCanton} sind keine Custom Settings verfügbar.");
        }

        if (!_eVotingConfig.CustomSettings[bfsCanton.ToString()].EVotingEnabledMunicipalitiesList
            .Contains(bfsMunicipality))
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

    private bool IsAbove18Years(Person person) => AgeUtil.CalculateAge(person.DateOfBirth, _clock.Today) >= 18;

    private bool HasSwissNationality(Person person) => person.Nationality?.Equals(SwissNationality, StringComparison.OrdinalIgnoreCase) == true;

    private bool HasReachedMaxAllowedVoters(short bfs, BfsStatistic bfsStatistic, bool required = true)
    {
        var settings = _eVotingConfig.CustomSettings.GetValueOrDefault(bfs.ToString());
        if (settings == null)
        {
            if (required)
            {
                throw new InvalidOperationException($"Für den Kunden mit BFS {bfs} sind keine Custom Settings verfügbar.");
            }

            return false;
        }

        var permittedVoterCount = bfsStatistic.VoterTotalCount * settings.MaxAllowedVotersPercent / 100;
        var hasReachedLimit = bfsStatistic.EVoterTotalCount >= permittedVoterCount;
        var triggerAlert = bfsStatistic.EVoterTotalCount >= (permittedVoterCount - _eVotingConfig.AlertRegistrationLimitEVoterOffset);

        if (triggerAlert)
        {
            DiagnosticsConfig.SetEVotingReachedMaxAllowedEVoters(
                bfs,
                bfsStatistic.EVoterTotalCount,
                bfsStatistic.VoterTotalCount,
                settings.MaxAllowedVotersPercent,
                permittedVoterCount);
        }

        return hasReachedLimit;
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
}
