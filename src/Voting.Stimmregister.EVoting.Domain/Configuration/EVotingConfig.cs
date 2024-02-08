// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;

namespace Voting.Stimmregister.EVoting.Domain.Configuration;

/// <summary>
/// Configuration model for E-Voting.
/// </summary>
public class EVotingConfig
{
    public Dictionary<string, EVotingCustomConfig> CustomSettings { get; set; } = new();

    /// <summary>
    /// Gets or sets the number of E-Voter registrations when an alert should be triggered before the max.
    /// allowed number of E-Voters is reached. Depending on the scaling, subsystems will be notified early
    /// to take action.
    /// Example: Municipality with 3000 citizens and a maximum E-Voting of 30% will be notified at
    /// the time when 850 citizens have registrered for E-Voting.
    /// </summary>
    public int AlertRegistrationLimitEVoterOffset { get; set; } = 50;
}
