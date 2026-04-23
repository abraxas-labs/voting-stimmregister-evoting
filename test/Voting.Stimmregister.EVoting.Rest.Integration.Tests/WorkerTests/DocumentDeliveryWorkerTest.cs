// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Voting.Lib.Scheduler;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmregister.EVoting.Core.Configuration;
using Voting.Stimmregister.EVoting.Core.HostedServices;
using Voting.Stimmregister.EVoting.Core.Services;
using Voting.Stimmregister.EVoting.Domain.Configuration;
using Voting.Stimmregister.EVoting.Domain.Models;
using Voting.Stimmregister.EVoting.Rest.Integration.Tests.MockData;
using Voting.Stimmregister.EVoting.Rest.Integration.Tests.Mocks;

namespace Voting.Stimmregister.EVoting.Rest.Integration.Tests.WorkerTests;

public class DocumentDeliveryWorkerTest : BaseTest
{
    public DocumentDeliveryWorkerTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    private List<(string FileName, string MessageType)> UploadedFiles => GetService<DokConnectorServiceMock>().UploadedFiles;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        UploadedFiles.Clear();
    }

    [Fact]
    public async Task ShouldWork()
    {
        var fileName = "test.pdf";

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
                },
                Document = new DocumentEntity
                {
                    CreatedAt = MockedClock.UtcNowDate.AddDays(-1),
                    Document = [1, 2, 3],
                    FileName = fileName,
                },
            });
            await db.SaveChangesAsync();
        });

        await RunWorkerWithDeliveryConfig(BfsCantonMockedData.BfsCantonValid);

        UploadedFiles.Should().HaveCount(1);
        UploadedFiles[0].Should().Be((fileName, "321"));
    }

    [Fact]
    public async Task ShouldWorkDifferentMessageType()
    {
        var fileName = "test2.pdf";

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
                    MunicipalityBfs = BfsMunicipalityMockedData.BfsAllowedWithEmail,
                    CantonBfs = BfsCantonMockedData.BfsCantonEmailRequired,
                },
                Document = new DocumentEntity
                {
                    CreatedAt = MockedClock.UtcNowDate.AddDays(-1),
                    Document = [1, 2, 3],
                    FileName = fileName,
                },
            });
            await db.SaveChangesAsync();
        });

        await RunWorkerWithDeliveryConfig(BfsCantonMockedData.BfsCantonEmailRequired);

        UploadedFiles.Should().HaveCount(1);
        UploadedFiles[0].Should().Be((fileName, "999"));
    }

    [Fact]
    public async Task ShouldIgnoreNotReadyStatusChanges()
    {
        await RunOnDb(async db =>
        {
            // Not active
            db.EVotingStatusChanges.Add(new EVotingStatusChangeEntity
            {
                Active = false,
                EVotingRegistered = false,
                ContextId = Guid.NewGuid().ToString(),
                CreatedAt = new DateTime(2024, 12, 3, 12, 13, 1, DateTimeKind.Utc),
                Person = new PersonEntity
                {
                    AllowedToVote = true,
                    Ahvn13 = 7561234567897,
                    DateOfBirth = new DateOnly(1990, 5, 23),
                    FirstName = "Max",
                    MunicipalityBfs = BfsMunicipalityMockedData.BfsAllowedWithEmail,
                    CantonBfs = BfsCantonMockedData.BfsCantonEmailRequired,
                },
                Document = new DocumentEntity
                {
                    CreatedAt = MockedClock.UtcNowDate.AddDays(-1),
                    Document = [1, 2, 3],
                    FileName = "test.pdf",
                },
            });

            // Created in the future
            db.EVotingStatusChanges.Add(new EVotingStatusChangeEntity
            {
                Active = true,
                EVotingRegistered = false,
                ContextId = Guid.NewGuid().ToString(),
                CreatedAt = new DateTime(2024, 12, 3, 12, 13, 1, DateTimeKind.Utc),
                Person = new PersonEntity
                {
                    AllowedToVote = true,
                    Ahvn13 = 7561234567897,
                    DateOfBirth = new DateOnly(1990, 5, 23),
                    FirstName = "Max",
                    MunicipalityBfs = BfsMunicipalityMockedData.BfsAllowedWithEmail,
                    CantonBfs = BfsCantonMockedData.BfsCantonEmailRequired,
                },
                Document = new DocumentEntity
                {
                    CreatedAt = MockedClock.UtcNowDate.AddDays(1),
                    Document = [1, 2, 3],
                    FileName = "test.pdf",
                },
            });

            // Without a document
            db.EVotingStatusChanges.Add(new EVotingStatusChangeEntity
            {
                Active = true,
                EVotingRegistered = true,
                ContextId = Guid.NewGuid().ToString(),
                CreatedAt = new DateTime(2024, 12, 3, 12, 13, 1, DateTimeKind.Utc),
                Person = new PersonEntity
                {
                    AllowedToVote = true,
                    Ahvn13 = 7561234567897,
                    DateOfBirth = new DateOnly(1990, 5, 23),
                    FirstName = "Max",
                    MunicipalityBfs = BfsMunicipalityMockedData.BfsAllowedWithEmail,
                    CantonBfs = BfsCantonMockedData.BfsCantonEmailRequired,
                },
            });
            await db.SaveChangesAsync();
        });

        await RunWorkerWithDeliveryConfig(BfsCantonMockedData.BfsCantonEmailRequired);

        UploadedFiles.Should().HaveCount(0);
    }

    [Fact]
    public async Task ShouldOnlyDeliverDocumentsForConfiguredTenant()
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
                },
                Document = new DocumentEntity
                {
                    CreatedAt = MockedClock.UtcNowDate.AddDays(-1),
                    Document = [1, 2, 3],
                    FileName = "sg.pdf",
                },
            });
            await db.SaveChangesAsync();
        });

        // Run the worker configured for canton TG (BFS 20) — it must not pick up the SG document
        await RunWorkerWithDeliveryConfig(BfsCantonMockedData.BfsCantonEmailRequired);

        UploadedFiles.Should().BeEmpty("canton TG worker must not deliver documents belonging to canton SG");
    }

    [Fact]
    public async Task ShouldThrowWhenDeliveryConfigIsNotSetInConfigAccessor()
    {
        using var scope = Factory.Services.CreateScope();
        var sp = scope.ServiceProvider;
        await Assert.ThrowsAsync<InvalidOperationException>(() => sp.GetRequiredService<DocumentDeliveryWorker>().Run(CancellationToken.None));
    }

    private async Task RunWorkerWithDeliveryConfig(short cantonBfs)
    {
        using var scope = Factory.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var eVotingConfig = sp.GetRequiredService<EVotingConfig>();
        var customConfig = eVotingConfig.CustomSettings[cantonBfs];
        var jobRunner = sp.GetRequiredService<JobRunner>();
        await jobRunner.RunJob<DocumentDeliveryJob, DocumentDeliveryConfig>(
            new DocumentDeliveryConfig
            {
                CantonBfs = cantonBfs,
                CronSchedule = customConfig.DeliveryCronSchedule,
                CronTimeZone = customConfig.DeliveryCronTimeZone,
            },
            CancellationToken.None);
    }
}
