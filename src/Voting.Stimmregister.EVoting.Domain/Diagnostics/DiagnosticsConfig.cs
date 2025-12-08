// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Prometheus;
using Voting.Stimmregister.EVoting.Domain.Enums;

namespace Voting.Stimmregister.EVoting.Domain.Diagnostics;

/// <summary>
/// A static Diagnostic class holding prometheus metrics instances and provides methods to update them accordingly.
/// </summary>
public static class DiagnosticsConfig
{
    private static readonly Gauge _eVotingRegistrationsMunicipality = Metrics
        .CreateGauge(
        "voting_stimmregister_evoting_registrations_municipality",
        "Number of active E-Voting registrations for a specific municipality.",
        labelNames: new[] { "municipalityBfs", "cantonBfs" });

    private static readonly Gauge _eVotingRegistrationsCanton = Metrics
        .CreateGauge(
        "voting_stimmregister_evoting_registrations_canton",
        "Number of active E-Voting registrations for a specific canton.",
        labelNames: new[] { "cantonBfs" });

    private static readonly Counter _deliveredDocuments = Metrics
        .CreateCounter(
        "voting_stimmregister_evoting_delivered_documents",
        "Count of successfully delivered documents.",
        labelNames: new[] { "status", "municipalityBfs" });

    private static readonly Counter _eVotingErrors = Metrics
        .CreateCounter(
        "voting_stimmregister_evoting_errors",
        "Count of business or technical errors.",
        labelNames: new[] { "status", "code" });

    private static readonly Gauge _eVotingRateLimit = Metrics
        .CreateGauge(
        "voting_stimmregister_evoting_rate_limit",
        "Size of the rate limit over time per user.",
        labelNames: new[] { "date", "id" });

    private static readonly Counter _eVotingReachedMaxAllowedEVoters = Metrics
        .CreateCounter(
        "voting_stimmregister_evoting_reached_max_allowed_voters",
        "BFS number that has reached the maximum allowed number of e-voter registrations.",
        labelNames: new[] { "bfs", "registeredEvoters", "eligibleVoters", "eligibleEVotersPercent", "eligibleEVoters" });

    private static readonly Gauge _activeStatusChanges = Metrics
        .CreateGauge(
            "voting_stimmregister_evoting_active_status_changes",
            "Count of active status changes.");

    private static readonly Gauge _oldestActiveStatusChangeAge = Metrics
        .CreateGauge(
            "voting_stimmregister_evoting_oldest_active_status_change_age",
            "Age of oldest active status change in hours.");

    private static readonly Counter _emailSendAttempts = Metrics
        .CreateCounter(
            "voting_stimmregister_evoting_email_send_attempts",
            "Count of attempts of sending emails.");

    private static readonly Counter _sentEmails = Metrics
        .CreateCounter(
            "voting_stimmregister_evoting_email_sent",
            "Count of of sent emails.");

    private static readonly Counter _errorEmails = Metrics
        .CreateCounter(
            "voting_stimmregister_evoting_email_errors",
            "Count of errored emails.");

    public static void SetEVotingRegistrations(double municipalityRegistrations, double cantonRegistrations, short municipalityBfs, short cantonBfs)
    {
        _eVotingRegistrationsMunicipality.WithLabels(municipalityBfs.ToString(), cantonBfs.ToString()).Set(municipalityRegistrations);
        _eVotingRegistrationsCanton.WithLabels(cantonBfs.ToString()).Set(cantonRegistrations);
    }

    public static void IncreaseDeliveredDocumentsCount(EVotingStatus status, short municipalityBfs)
    {
        _deliveredDocuments.WithLabels(status.ToString(), municipalityBfs.ToString()).Inc();
    }

    public static void IncreaseEVotingError(string status, int code)
    {
        _eVotingErrors.WithLabels(status, code.ToString()).Inc();
    }

    public static void SetRateLimit(int actionCount, string date, string id)
    {
        _eVotingRateLimit.WithLabels(date, id).Set(actionCount);
    }

    public static void SetEVotingReachedMaxAllowedEVoters(
        short bfs,
        int registeredEVoters,
        int eligibleVoters,
        int eligibleEVotersPercent,
        int eligibleEVoters)
    {
        _eVotingReachedMaxAllowedEVoters.WithLabels(
            bfs.ToString(),
            registeredEVoters.ToString(),
            eligibleVoters.ToString(),
            eligibleEVotersPercent.ToString(),
            eligibleEVoters.ToString()).IncTo(1);
    }

    public static void SetActiveStatusChanges(int count)
    {
        _activeStatusChanges.Set(count);
    }

    public static void SetOldestActiveStatusChangeAge(int ageInHours)
    {
        _oldestActiveStatusChangeAge.Set(ageInHours);
    }

    public static void IncreaseEmailSendAttempts()
    {
        _emailSendAttempts.Inc();
    }

    public static void IncreaseSentEmails()
    {
        _sentEmails.Inc();
    }

    public static void IncreaseEmailErrors()
    {
        _errorEmails.Inc();
    }
}
