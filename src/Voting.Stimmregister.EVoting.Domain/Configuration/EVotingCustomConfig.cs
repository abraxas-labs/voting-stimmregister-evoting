// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.Linq;

namespace Voting.Stimmregister.EVoting.Domain.Configuration;

public class EVotingCustomConfig
{
    /// <summary>
    /// Gets or sets the maximum percentage of voters that are allowed to register for e-voting.
    /// </summary>
    public int MaxAllowedVotersPercent { get; set; }

    public string EVotingEnabledMunicipalities { get; set; } = string.Empty;

    public ICollection<short> EVotingEnabledMunicipalitiesList => EVotingEnabledMunicipalities
        .Split(',', System.StringSplitOptions.RemoveEmptyEntries)
        .Select(e => short.Parse(e.Trim()))
        .ToList();
}
