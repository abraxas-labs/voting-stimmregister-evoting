// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmregister.EVoting.Domain.Enums;

/// <summary>
/// Enumeration defining the voters eVoting permisison.
/// </summary>
public enum VotingRight
{
    /// <summary>
    /// E-Voting permission is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// E-Voting permission is given.
    /// </summary>
    Permitted = 1,

    /// <summary>
    /// E-Voting permission is forbidden.
    /// </summary>
    NotPermitted = 2,
}
