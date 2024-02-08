// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmregister.EVoting.Domain.Enums;

/// <summary>
/// Enumeration defining the process status code.
/// </summary>
public enum ProcessStatusCode
{
    /// <summary>
    /// Unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Success.
    /// </summary>
    Success = 100,

    /// <summary>
    /// Invalid AHV number N13 Format.
    /// </summary>
    InvalidAhvn13Format = 400,

    /// <summary>
    /// Invalid BFS canton number format.
    /// </summary>
    InvalidBfsCantonFormat = 401,

    /// <summary>
    /// The date of birth does not match.
    /// </summary>
    DateOfBirthDoesNotMatch = 402,

    /// <summary>
    /// Person not found.
    /// </summary>
    PersonNotFound = 404,

    /// <summary>
    /// E-Voting permission error.
    /// </summary>
    EVotingPermissionError = 410,

    /// <summary>
    /// E-Voting reached max allowed voters limit.
    /// </summary>
    EVotingReachedMaxAllowedVoters = 411,

    /// <summary>
    /// Error indicating E-Voting is not enabled for the municipality.
    /// </summary>
    EVotingNotEnabledError = 412,

    /// <summary>
    /// Error indicating the current person is already registered for E-Voting.
    /// </summary>
    EVotingAlreadyRegistered = 413,

    /// <summary>
    /// Error indicating the current person is already unregistered for E-Voting or its E-Voting state is unknown.
    /// </summary>
    EVotingAlreadyUnregisteredOrUnknown = 414,

    /// <summary>
    /// No Context id in the HTTP-Headers provided when it was required.
    /// </summary>
    ContextNotProvided = 415,

    /// <summary>
    /// The rate limit has been exceeded.
    /// </summary>
    RateLimitExceeded = 416,

    /// <summary>
    /// Loganto organisation unit not found.
    /// </summary>
    LogantoOrganisationUnitNotFound = 430,

    /// <summary>
    /// Loganto service request error.
    /// </summary>
    LogantoServiceRequestError = 431,

    /// <summary>
    /// Loganto service data error.
    /// </summary>
    LogantoServiceDataError = 432,

    /// <summary>
    /// Loganto service business error.
    /// </summary>
    LogantoServiceBusinessError = 433,

    /// <summary>
    /// KEWR service request error.
    /// </summary>
    KewrServiceRequestError = 450,

    /// <summary>
    /// KEWR Service data error.
    /// </summary>
    KewrServiceDataError = 451,

    /// <summary>
    /// KEWR Service person error.
    /// </summary>
    KewrServicePersonError = 452,
}
