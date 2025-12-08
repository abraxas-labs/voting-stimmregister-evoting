// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Voting.Lib.Testing.Mocks;
using Voting.Stimmregister.EVoting.Adapter.Stimmregister.Models;
using Voting.Stimmregister.EVoting.Domain.Converters;
using Voting.Stimmregister.EVoting.Domain.Enums;
using Voting.Stimmregister.EVoting.Domain.Models;
using Voting.Stimmregister.EVoting.Rest.Integration.Tests.MockData;
using Voting.Stimmregister.EVoting.Rest.Integration.Tests.Mocks;
using Voting.Stimmregister.EVoting.Rest.Models.Request;

namespace Voting.Stimmregister.EVoting.Rest.Integration.Tests.RegistrationTests;

public class EmailVerificationTest : BaseRestTest
{
    private const string VerifyEmailApiUrl = "v1/verify-email";

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter(), new DateOnlyJsonConverter() },
        PropertyNameCaseInsensitive = true,
    };

    public EmailVerificationTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        var email = "test@example.invalid";
        var code = "testcode";
        var dateOfBirth = new DateOnly(1950, 01, 23);

        await RunOnDb(async db =>
        {
            db.EmailVerifications.Add(new EmailVerificationEntry
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1,
                Email = email,
                VerificationCode = code,
                BfsCanton = BfsCantonMockedData.BfsCantonEmailRequired,
                CreatedAt = MockedClock.UtcNowDate,
                DateOfBirth = dateOfBirth,
            });
            await db.SaveChangesAsync();
        });

        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Unregistered,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            true,
            BfsMunicipalityMockedData.BfsAllowedWithEmail);
        HttpClientFactoryMock.StimmregisterRegisterResponse = HttpClientFactoryMock.CreateOkStimmregisterResponse();

        using var resp = await VerifyEmail(BfsCantonMockedData.BfsCantonEmailRequired, code);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var statusChangeExists = await RunOnDb(db =>
        {
            return db.EVotingStatusChanges.AnyAsync(c =>
                c.EVotingRegistered
                && c.Active
                && c.Person!.Ahvn13 == Ahvn13MockedData.Ahvn13Valid1);
        });
        statusChangeExists.Should().Be(true);

        var emailVerificationExists = await RunOnDb(db => db.EmailVerifications.AnyAsync(x => x.Ahvn13 == Ahvn13MockedData.Ahvn13Valid1));
        emailVerificationExists.Should().Be(false);
    }

    [Fact]
    public async Task ShouldWorkWithExistingActiveStatusChange()
    {
        var email = "test@example.invalid";
        var code = "testcode";
        var dateOfBirth = new DateOnly(1950, 01, 23);

        await RunOnDb(async db =>
        {
            db.EmailVerifications.Add(new EmailVerificationEntry
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1,
                Email = email,
                VerificationCode = code,
                BfsCanton = BfsCantonMockedData.BfsCantonEmailRequired,
                CreatedAt = MockedClock.UtcNowDate,
                DateOfBirth = dateOfBirth,
            });
            db.EVotingStatusChanges.Add(new EVotingStatusChangeEntity
            {
                EVotingRegistered = false,
                Active = true,
                ContextId = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                Person = new PersonEntity
                {
                    Ahvn13 = Ahvn13MockedData.Ahvn13Valid1,
                    DateOfBirth = dateOfBirth,
                },
                Document = new DocumentEntity
                {
                    Document = new byte[] { 1, 2, 3 },
                    CreatedAt = DateTime.UtcNow,
                    FileName = "test.pdf",
                },
            });
            await db.SaveChangesAsync();
        });

        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Unregistered,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            true,
            BfsMunicipalityMockedData.BfsAllowedWithEmail);
        HttpClientFactoryMock.StimmregisterRegisterResponse = HttpClientFactoryMock.CreateOkStimmregisterResponse();

        using var resp = await VerifyEmail(BfsCantonMockedData.BfsCantonEmailRequired, code);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var statusChanges = await RunOnDb(db =>
        {
            return db.EVotingStatusChanges
                .Include(c => c.Document)
                .Where(c => c.Person!.Ahvn13 == Ahvn13MockedData.Ahvn13Valid1)
                .OrderBy(c => c.EVotingRegistered)
                .ToListAsync();
        });

        statusChanges.Count.Should().Be(2);

        var unregistered = statusChanges[0];
        unregistered.EVotingRegistered.Should().BeFalse();
        unregistered.Document.Should().BeNull();
        unregistered.Active.Should().BeFalse();

        var registered = statusChanges[1];
        registered.EVotingRegistered.Should().BeTrue();
        registered.Active.Should().BeFalse();
    }

    [Fact]
    public async Task ShouldWorkWithInactiveStatusChange()
    {
        var email = "test@example.invalid";
        var code = "testcode";
        var dateOfBirth = new DateOnly(1950, 01, 23);

        await RunOnDb(async db =>
        {
            db.EmailVerifications.Add(new EmailVerificationEntry
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1,
                Email = email,
                VerificationCode = code,
                BfsCanton = BfsCantonMockedData.BfsCantonEmailRequired,
                CreatedAt = MockedClock.UtcNowDate,
                DateOfBirth = dateOfBirth,
            });

            db.EVotingStatusChanges.Add(new EVotingStatusChangeEntity
            {
                EVotingRegistered = false,
                Active = false,
                ContextId = Guid.NewGuid().ToString(),
                CreatedAt = MockedClock.UtcNowDate,
                Person = new PersonEntity
                {
                    Ahvn13 = Ahvn13MockedData.Ahvn13Valid1,
                    DateOfBirth = dateOfBirth,
                },
            });
            await db.SaveChangesAsync();
        });

        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Unregistered,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            true,
            BfsMunicipalityMockedData.BfsAllowedWithEmail);
        HttpClientFactoryMock.StimmregisterRegisterResponse = HttpClientFactoryMock.CreateOkStimmregisterResponse();

        using var resp = await VerifyEmail(BfsCantonMockedData.BfsCantonEmailRequired, code);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var statusChanges = await RunOnDb(db =>
        {
            return db.EVotingStatusChanges
                .Include(c => c.Document)
                .Where(c => c.Person!.Ahvn13 == Ahvn13MockedData.Ahvn13Valid1)
                .OrderBy(c => c.EVotingRegistered)
                .ToListAsync();
        });

        statusChanges.Count.Should().Be(2);

        var unregistered = statusChanges[0];
        unregistered.EVotingRegistered.Should().BeFalse();
        unregistered.Active.Should().BeFalse();

        var registered = statusChanges[1];
        registered.EVotingRegistered.Should().BeTrue();
        registered.Active.Should().BeTrue();
    }

    [Fact]
    public async Task ShouldReturnErrorWhenVerificationIsExpired()
    {
        var email = "test@example.invalid";
        var code = "testcode";
        var dateOfBirth = new DateOnly(1950, 01, 23);

        await RunOnDb(async db =>
        {
            db.EmailVerifications.Add(new EmailVerificationEntry
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1,
                Email = email,
                VerificationCode = code,
                BfsCanton = BfsCantonMockedData.BfsCantonEmailRequired,
                CreatedAt = MockedClock.UtcNowDate.AddDays(-10),
                DateOfBirth = dateOfBirth,
            });
            await db.SaveChangesAsync();
        });

        using var resp = await VerifyEmail(BfsCantonMockedData.BfsCantonEmailRequired, code);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await resp.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.EmailVerificationValidityExpired);
    }

    [Fact]
    public async Task ShouldReturnErrorWhenCodeDoesNotMatch()
    {
        var email = "test@example.invalid";
        var dateOfBirth = new DateOnly(1950, 01, 23);

        await RunOnDb(async db =>
        {
            db.EmailVerifications.Add(new EmailVerificationEntry
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1,
                Email = email,
                VerificationCode = "test",
                BfsCanton = BfsCantonMockedData.BfsCantonEmailRequired,
                CreatedAt = MockedClock.UtcNowDate,
                DateOfBirth = dateOfBirth,
            });
            await db.SaveChangesAsync();
        });

        using var resp = await VerifyEmail(BfsCantonMockedData.BfsCantonEmailRequired, "other");

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await resp.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.EmailVerificationFailed);
    }

    [Fact]
    public async Task ShouldReturnErrorWhenBfsDoesNotMatch()
    {
        var email = "test@example.invalid";
        var dateOfBirth = new DateOnly(1950, 01, 23);

        await RunOnDb(async db =>
        {
            db.EmailVerifications.Add(new EmailVerificationEntry
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1,
                Email = email,
                VerificationCode = "test",
                BfsCanton = BfsCantonMockedData.BfsCantonEmailRequired,
                CreatedAt = MockedClock.UtcNowDate,
                DateOfBirth = dateOfBirth,
            });
            await db.SaveChangesAsync();
        });

        using var resp = await VerifyEmail(BfsCantonMockedData.BfsCantonValid, "test");

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await resp.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.EmailVerificationFailed);
    }

    [Fact]
    public async Task ShouldReturnErrorWhenNotAllowedToVote()
    {
        var email = "test@example.invalid";
        var code = "testcode";
        var dateOfBirth = new DateOnly(1950, 01, 23);

        await RunOnDb(async db =>
        {
            db.EmailVerifications.Add(new EmailVerificationEntry
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1,
                Email = email,
                VerificationCode = code,
                BfsCanton = BfsCantonMockedData.BfsCantonEmailRequired,
                CreatedAt = MockedClock.UtcNowDate,
                DateOfBirth = dateOfBirth,
            });
            await db.SaveChangesAsync();
        });

        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Unknown,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            false,
            BfsMunicipalityMockedData.BfsAllowedWithEmail);

        using var resp = await VerifyEmail(BfsCantonMockedData.BfsCantonEmailRequired, code);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await resp.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.EVotingPermissionError);
    }

    [Fact]
    public async Task ShouldReturnErrorWhenCantonLimitReached()
    {
        var email = "test@example.invalid";
        var code = "testcode";
        var dateOfBirth = new DateOnly(1950, 01, 23);

        await RunOnDb(async db =>
        {
            db.EmailVerifications.Add(new EmailVerificationEntry
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1,
                Email = email,
                VerificationCode = code,
                BfsCanton = BfsCantonMockedData.BfsCantonEmailRequired,
                CreatedAt = MockedClock.UtcNowDate,
                DateOfBirth = dateOfBirth,
            });
            await db.SaveChangesAsync();
        });

        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Unregistered,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            true,
            BfsMunicipalityMockedData.BfsAllowedWithEmail,
            "Schweiz",
            900);

        using var resp = await VerifyEmail(BfsCantonMockedData.BfsCantonEmailRequired, code);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await resp.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.EVotingReachedMaxAllowedVoters);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenPassingNoContent()
    {
        await AssertStatus(
            () => AdminClient.PostAsJsonAsync(VerifyEmailApiUrl, new { }),
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldWorkWithEmailChange()
    {
        var email = "test@example.invalid";
        var code = "testcode";
        var dateOfBirth = new DateOnly(1950, 01, 23);

        await RunOnDb(async db =>
        {
            db.EmailVerifications.Add(new EmailVerificationEntry
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1,
                Email = email,
                VerificationCode = code,
                BfsCanton = BfsCantonMockedData.BfsCantonEmailRequired,
                CreatedAt = MockedClock.UtcNowDate,
                DateOfBirth = dateOfBirth,
                IsEmailChange = true,
            });
            await db.SaveChangesAsync();
        });

        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Registered,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            true,
            BfsMunicipalityMockedData.BfsAllowedWithEmail);
        HttpClientFactoryMock.StimmregisterRegisterResponse = HttpClientFactoryMock.CreateOkStimmregisterResponse();

        using var resp = await VerifyEmail(BfsCantonMockedData.BfsCantonEmailRequired, code);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var statusChangeExists = await RunOnDb(db =>
        {
            return db.EVotingStatusChanges.AnyAsync(c =>
                c.EVotingRegistered
                && c.Active
                && c.Person!.Ahvn13 == Ahvn13MockedData.Ahvn13Valid1
                && c.Person.Email == email);
        });
        statusChangeExists.Should().Be(true);

        var emailVerificationExists = await RunOnDb(db => db.EmailVerifications.AnyAsync(x => x.Ahvn13 == Ahvn13MockedData.Ahvn13Valid1));
        emailVerificationExists.Should().Be(false);
    }

    [Fact]
    public async Task ShouldWorkWithExistingActiveStatusChangeWithEmailChange()
    {
        var email = "test@example.invalid";
        var code = "testcode";
        var dateOfBirth = new DateOnly(1950, 01, 23);

        await RunOnDb(async db =>
        {
            db.EmailVerifications.Add(new EmailVerificationEntry
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1,
                Email = email,
                VerificationCode = code,
                BfsCanton = BfsCantonMockedData.BfsCantonEmailRequired,
                CreatedAt = MockedClock.UtcNowDate,
                DateOfBirth = dateOfBirth,
                IsEmailChange = true,
            });
            db.EVotingStatusChanges.Add(new EVotingStatusChangeEntity
            {
                EVotingRegistered = true,
                Active = true,
                ContextId = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                Person = new PersonEntity
                {
                    Ahvn13 = Ahvn13MockedData.Ahvn13Valid1,
                    DateOfBirth = dateOfBirth,
                },
                Document = new DocumentEntity
                {
                    Document = new byte[] { 1, 2, 3 },
                    CreatedAt = DateTime.UtcNow,
                    FileName = "test.pdf",
                },
            });
            await db.SaveChangesAsync();
        });

        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Registered,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            true,
            BfsMunicipalityMockedData.BfsAllowedWithEmail);
        HttpClientFactoryMock.StimmregisterRegisterResponse = HttpClientFactoryMock.CreateOkStimmregisterResponse();

        using var resp = await VerifyEmail(BfsCantonMockedData.BfsCantonEmailRequired, code);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var statusChanges = await RunOnDb(db =>
        {
            return db.EVotingStatusChanges
                .Include(c => c.Document)
                .Where(c => c.Person!.Ahvn13 == Ahvn13MockedData.Ahvn13Valid1)
                .OrderByDescending(c => c.Active)
                .ToListAsync();
        });

        statusChanges.Count.Should().Be(2);

        var unregistered = statusChanges[0];
        unregistered.EVotingRegistered.Should().BeTrue();
        unregistered.Active.Should().BeTrue();

        var registered = statusChanges[1];
        registered.EVotingRegistered.Should().BeTrue();
        registered.Active.Should().BeFalse();
        unregistered.Document.Should().BeNull();
    }

    protected override Task<HttpResponseMessage> AuthorizationTestCall(HttpClient httpClient)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, VerifyEmailApiUrl);
        request.Content = JsonContent.Create(
            new VerifyEmailRequest
            {
                BfsCanton = BfsCantonMockedData.BfsCantonEmailRequired,
                Code = "test",
            },
            options: _jsonOptions);
        request.Headers.Add("X-Context-Id", Guid.NewGuid().ToString());
        return httpClient.SendAsync(request);
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return "unkown";
    }

    private async Task<HttpResponseMessage> VerifyEmail(short bfsCanton, string code)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, VerifyEmailApiUrl);
        request.Content = JsonContent.Create(
            new VerifyEmailRequest
            {
                BfsCanton = bfsCanton,
                Code = code,
            },
            options: _jsonOptions);
        request.Headers.Add("X-Context-Id", Guid.NewGuid().ToString());
        return await AdminClient.SendAsync(request);
    }
}
