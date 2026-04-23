// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using FluentAssertions;
using Voting.Stimmregister.EVoting.Domain.Configuration;
using Xunit;

namespace Voting.Stimmregister.EVoting.Core.Unit.Tests.ConfigurationTests;

public class EVotingCustomConfigTests
{
    [Fact]
    public void Validate_ShouldThrow_WhenDeliveryCronScheduleIsMissing()
    {
        var config = new EVotingCustomConfig
        {
            CantonName = "Test Canton",
            MaxAllowedVotersPercent = 30,
            EVotingEnabledMunicipalities = "1",
            ConnectorMessageType = "123",
            DeliveryCronSchedule = string.Empty,
        };

        var act = () => config.Validate();

        act.Should().Throw<ArgumentException>()
            .WithMessage("Value cannot be null*");
    }

    [Fact]
    public void Validate_ShouldThrow_WhenDeliveryCronScheduleIsWhitespace()
    {
        var config = new EVotingCustomConfig
        {
            CantonName = "Test Canton",
            MaxAllowedVotersPercent = 30,
            EVotingEnabledMunicipalities = "1",
            ConnectorMessageType = "123",
            DeliveryCronSchedule = "   ",
        };

        var act = () => config.Validate();

        act.Should().Throw<ArgumentException>()
            .WithMessage("*DeliveryCronSchedule*");
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenDeliveryCronScheduleIsSet()
    {
        var config = new EVotingCustomConfig
        {
            CantonName = "St. Gallen",
            MaxAllowedVotersPercent = 30,
            EVotingEnabledMunicipalities = "1",
            ConnectorMessageType = "321",
            DeliveryCronSchedule = "30 9 * * *",
        };

        var act = () => config.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_ShouldThrow_WhenEmailRequiredButCallbackUrlMissing()
    {
        var config = new EVotingCustomConfig
        {
            CantonName = "Thurgau",
            MaxAllowedVotersPercent = 30,
            EVotingEnabledMunicipalities = "3",
            ConnectorMessageType = "999",
            DeliveryCronSchedule = "0 10 * * *",
            RequiresEmail = true,
            EmailVerificationCallbackUrl = string.Empty,
        };

        var act = () => config.Validate();

        act.Should().Throw<ArgumentException>();
    }
}
