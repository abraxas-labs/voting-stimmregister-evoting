// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Testing.Utils;
using Voting.Stimmregister.EVoting.Core.Services;
using Voting.Stimmregister.EVoting.Domain.Enums;
using Voting.Stimmregister.EVoting.Domain.Models;
using Voting.Stimmregister.EVoting.Rest.Integration.Tests.MockData;
using Voting.Stimmregister.EVoting.Rest.Integration.Tests.Mocks;

namespace Voting.Stimmregister.EVoting.Rest.Integration.Tests.WorkerTests;

public class DocumentGeneratorWorkerTest : BaseTest
{
    public DocumentGeneratorWorkerTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    private List<string> GeneratedTemplateKeys => GetService<PdfServiceMock>().Generated;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        GeneratedTemplateKeys.Clear();
    }

    [Fact]
    public async Task ShouldWorkRegister()
    {
        await RunOnDb(async db =>
        {
            db.EVotingStatusChanges.Add(new EVotingStatusChangeEntity
            {
                Active = true,
                EVotingRegistered = true,
                CreatedAt = new DateTime(2024, 12, 3, 12, 13, 1, DateTimeKind.Utc),
                Person = new PersonEntity
                {
                    AllowedToVote = true,
                    Ahvn13 = 7561234567897,
                    DateOfBirth = new DateOnly(1990, 5, 23),
                    FirstName = "Max",
                    MunicipalityBfs = BfsMunicipalityMockedData.BfsAllowedForEVoting,
                    CantonBfs = BfsCantonMockedData.BfsCantonValid,
                    Nationality = "Schweiz",
                    OfficialName = "Muster",
                    Sex = Sex.Male,
                    Address = new AddressEntity
                    {
                        HouseNumber = "3a",
                        Street = "Hauptstrasse",
                        PostOfficeBoxText = "Postfach 3",
                        Town = "St. Gallen",
                        ZipCode = "9000",
                    },
                },
            });
            await db.SaveChangesAsync();
        });

        var worker = GetService<DocumentGeneratorWorker>();
        await worker.Run(CancellationToken.None);

        await TestXml("evoting_brief_anmeldung");
    }

    [Fact]
    public async Task ShouldWorkRegisterWithEmail()
    {
        await RunOnDb(async db =>
        {
            db.EVotingStatusChanges.Add(new EVotingStatusChangeEntity
            {
                Active = true,
                EVotingRegistered = true,
                CreatedAt = new DateTime(2024, 12, 3, 12, 13, 1, DateTimeKind.Utc),
                Person = new PersonEntity
                {
                    AllowedToVote = true,
                    Ahvn13 = 7561234567897,
                    DateOfBirth = new DateOnly(1993, 5, 23),
                    FirstName = "Frida",
                    MunicipalityBfs = BfsMunicipalityMockedData.BfsAllowedWithEmail,
                    CantonBfs = BfsCantonMockedData.BfsCantonEmailRequired,
                    Nationality = "Schweiz",
                    OfficialName = "Friedenfrau",
                    Email = "frida@test.invalid",
                    Sex = Sex.Male,
                    Address = new AddressEntity
                    {
                        HouseNumber = "3a",
                        Street = "Hauptstrasse",
                        PostOfficeBoxText = "Postfach 3",
                        Town = "St. Gallen",
                        ZipCode = "9000",
                    },
                },
            });
            await db.SaveChangesAsync();
        });

        var worker = GetService<DocumentGeneratorWorker>();
        await worker.Run(CancellationToken.None);

        await TestXml("evoting_brief_anmeldung_tg");
    }

    [Fact]
    public async Task ShouldWorkUnregister()
    {
        await RunOnDb(async db =>
        {
            db.EVotingStatusChanges.Add(new EVotingStatusChangeEntity
            {
                Active = true,
                EVotingRegistered = false,
                CreatedAt = new DateTime(2024, 12, 3, 12, 13, 1, DateTimeKind.Utc),
                Person = new PersonEntity
                {
                    AllowedToVote = true,
                    Ahvn13 = 7561234567897,
                    DateOfBirth = new DateOnly(1990, 5, 23),
                    FirstName = "Max",
                    MunicipalityBfs = BfsMunicipalityMockedData.BfsAllowedForEVoting,
                    CantonBfs = BfsCantonMockedData.BfsCantonValid,
                    Nationality = "Schweiz",
                    OfficialName = "Muster",
                    Sex = Sex.Male,
                    Address = new AddressEntity
                    {
                        HouseNumber = "3a",
                        Street = "Hauptstrasse",
                        PostOfficeBoxText = "Postfach 3",
                        Town = "St. Gallen",
                        ZipCode = "9000",
                    },
                },
            });
            await db.SaveChangesAsync();
        });

        var worker = GetService<DocumentGeneratorWorker>();
        await worker.Run(CancellationToken.None);

        await TestXml("evoting_brief_abmeldung");
    }

    [Fact]
    public async Task ShouldWorkUnregisterWithEmail()
    {
        await RunOnDb(async db =>
        {
            db.EVotingStatusChanges.Add(new EVotingStatusChangeEntity
            {
                Active = true,
                EVotingRegistered = false,
                CreatedAt = new DateTime(2024, 12, 3, 12, 13, 1, DateTimeKind.Utc),
                Person = new PersonEntity
                {
                    AllowedToVote = true,
                    Ahvn13 = 7561234567897,
                    DateOfBirth = new DateOnly(1993, 5, 23),
                    FirstName = "Frida",
                    MunicipalityBfs = BfsMunicipalityMockedData.BfsAllowedWithEmail,
                    CantonBfs = BfsCantonMockedData.BfsCantonEmailRequired,
                    Nationality = "Schweiz",
                    OfficialName = "Friedenfrau",
                    Email = "frida@test.invalid",
                    Sex = Sex.Male,
                    Address = new AddressEntity
                    {
                        HouseNumber = "3a",
                        Street = "Hauptstrasse",
                        PostOfficeBoxText = "Postfach 3",
                        Town = "St. Gallen",
                        ZipCode = "9000",
                    },
                },
            });
            await db.SaveChangesAsync();
        });

        var worker = GetService<DocumentGeneratorWorker>();
        await worker.Run(CancellationToken.None);

        await TestXml("evoting_brief_abmeldung_tg");
    }

    [Fact]
    public async Task ShouldIgnoreInactiveStatusChanges()
    {
        await RunOnDb(async db =>
        {
            db.EVotingStatusChanges.Add(new EVotingStatusChangeEntity
            {
                Active = false,
                EVotingRegistered = false,
                CreatedAt = new DateTime(2024, 12, 3, 12, 13, 1, DateTimeKind.Utc),
                Person = new PersonEntity
                {
                    AllowedToVote = true,
                    Ahvn13 = 7561234567897,
                    DateOfBirth = new DateOnly(1993, 5, 23),
                    FirstName = "Frida",
                },
            });
            await db.SaveChangesAsync();
        });

        var worker = GetService<DocumentGeneratorWorker>();
        await worker.Run(CancellationToken.None);

        GeneratedTemplateKeys.Should().HaveCount(0);

        var hasDocuments = await RunOnDb(db => db.Documents.AnyAsync());
        hasDocuments.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldIgnoreStatusChangesWithDocuments()
    {
        await RunOnDb(async db =>
        {
            db.EVotingStatusChanges.Add(new EVotingStatusChangeEntity
            {
                Active = false,
                EVotingRegistered = false,
                CreatedAt = new DateTime(2024, 12, 3, 12, 13, 1, DateTimeKind.Utc),
                Person = new PersonEntity
                {
                    AllowedToVote = true,
                    Ahvn13 = 7561234567897,
                    DateOfBirth = new DateOnly(1993, 5, 23),
                    FirstName = "Frida",
                },
                Document = new DocumentEntity
                {
                    Document = [1, 2, 3],
                    FileName = "test.pdf",
                    WorkerName = "testing",
                },
            });
            await db.SaveChangesAsync();
        });

        var worker = GetService<DocumentGeneratorWorker>();
        await worker.Run(CancellationToken.None);

        GeneratedTemplateKeys.Should().HaveCount(0);
    }

    private async Task TestXml(string templateKey, [CallerMemberName] string memberName = "")
    {
        GeneratedTemplateKeys.Should().HaveCount(1);
        GeneratedTemplateKeys[0].Should().Be(templateKey);

        var document = await RunOnDb(db => db.Documents.SingleAsync());

        var xml = Encoding.UTF8.GetString(document.Document!);

#if UPDATE_SNAPSHOTS
        var updateSnapshot = true;
#else
        var updateSnapshot = false;
#endif

        xml.MatchXmlSnapshot(updateSnapshot: updateSnapshot, memberName: memberName);
    }
}
