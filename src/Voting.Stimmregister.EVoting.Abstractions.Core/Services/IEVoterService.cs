// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Threading;
using System.Threading.Tasks;
using Voting.Stimmregister.EVoting.Domain.Enums;
using Voting.Stimmregister.EVoting.Domain.Models;

namespace Voting.Stimmregister.EVoting.Abstractions.Core.Services;

public interface IEVoterService
{
    bool EmailRequired { get; }

    /// <summary>
    /// Checks the e-voting status of a person.
    /// </summary>
    /// <param name="personIdentification">The person information.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The e-voting status of the person.</returns>
    Task<EVotingStatusModel> GetEVotingStatus(PersonIdentification personIdentification, CancellationToken ct);

    /// <summary>
    /// Registers a person for e-voting.
    /// If an email is required, only a verification email is sent. The actual registration is later performed by <see cref="VerifyEmail"/>.
    /// </summary>
    /// <param name="personIdentification">The person information.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The process status.</returns>
    Task<ProcessStatusCode> Register(PersonIdentification personIdentification, CancellationToken ct);

    /// <summary>
    /// Unregisters a person from e-voting.
    /// </summary>
    /// <param name="personIdentification">The person information.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task Unregister(PersonIdentification personIdentification, CancellationToken ct);

    /// <summary>
    /// Initializes the email change flow. A verification email will be sent to the new email address.
    /// Only works if the person is already a registered e-voter.
    /// </summary>
    /// <param name="personIdentification">The person information.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ChangeEmail(PersonIdentification personIdentification, CancellationToken ct);

    /// <summary>
    /// Verifies the email. If successful, performs the task that was pending for the email verification (e.g. register or change email).
    /// </summary>
    /// <param name="verificationCode">The email verification code.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task VerifyEmail(string verificationCode, CancellationToken ct);
}
