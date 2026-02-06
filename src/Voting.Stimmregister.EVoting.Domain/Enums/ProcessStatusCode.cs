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
    /// Successful registration, but email validation is pending.
    /// </summary>
    SuccessWithPendingEmailVerification = 101,

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
    /// Invalid email format.
    /// </summary>
    InvalidEmailFormat = 403,

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
    /// Error indicating the current person is already pending for the E-Voting registration.
    /// </summary>
    EVotingAlreadyPendingRegistration = 417,

    /// <summary>
    /// Error indicating that the email verification failed.
    /// </summary>
    EmailVerificationFailed = 418,

    /// <summary>
    /// Error indicating that the email verification took too long and expired.
    /// </summary>
    EmailVerificationValidityExpired = 419,

    /// <summary>
    /// Error indicating that a user wants to change the email address, but the system does not support this.
    /// </summary>
    EmailCannotBeChanged = 420,

    /// <summary>
    /// The email change rate limit has been exceeded.
    /// </summary>
    EmailChangeRateLimitExceeded = 421,
}
