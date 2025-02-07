// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using FluentAssertions;
using Voting.Stimmregister.EVoting.Adapter.Document.Mapping;
using Voting.Stimmregister.EVoting.Domain.Models;
using Xunit;

namespace Voting.Stimmregister.EVoting.Core.Unit.Tests.MapperTests;

public class TemplateMapperTest
{
    [Fact]
    public void ShouldMapStreetWhenStreetSet()
    {
        TemplateMapper
            .MapToEVotingInformation(CreateValidPersonEntity(p => p.Address.PostOfficeBoxText = string.Empty), true)
            .Person!.Address!.Street
            .Should().Be("TestStreet");
    }

    [Fact]
    public void ShouldMapStreetWhenStreetAndPostOfficeBoxTextSet()
    {
        TemplateMapper
            .MapToEVotingInformation(CreateValidPersonEntity(), true)
            .Person!.Address!.Street
            .Should().Be("TestStreet");
    }

    [Fact]
    public void ShouldMapPostOfficeBoxTextWhenStreetNotSet()
    {
        TemplateMapper
            .MapToEVotingInformation(CreateValidPersonEntity(p => p.Address.Street = string.Empty), true)
            .Person!.Address!.Street
            .Should().Be("TestPostOfficeBoxText");
    }

    [Fact]
    public void ShouldMapEmptyWhenNoAddressInfoSet()
    {
        TemplateMapper
            .MapToEVotingInformation(
                CreateValidPersonEntity(p =>
                {
                    p.Address.Street = string.Empty;
                    p.Address.PostOfficeBoxText = string.Empty;
                }),
                true)
            .Person!.Address!.Street
            .Should().BeEmpty();
    }

    private static PersonEntity CreateValidPersonEntity(Action<PersonEntity>? action = null)
    {
        var personEntity = new PersonEntity
        {
            Id = Guid.NewGuid(),
            AllowedToVote = true,
            Ahvn13 = 1234567890123,
            DateOfBirth = new DateOnly(2000, 1, 1),
            FirstName = "TestFirstName",
            OfficialName = "TestOfficialName",
            MunicipalityBfs = 1234,
            Nationality = "TestNationality",
            Sex = Domain.Enums.Sex.Female,
            Address = new AddressEntity
            {
                Street = "TestStreet",
                PostOfficeBoxText = "TestPostOfficeBoxText",
                HouseNumber = "TestHouseNumber",
                Town = "TestTown",
                ZipCode = "TestZipCode",
            },
        };

        action?.Invoke(personEntity);
        return personEntity;
    }
}
