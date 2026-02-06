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
using Voting.Lib.UserNotifications;
using Voting.Stimmregister.EVoting.Adapter.Stimmregister.Models;
using Voting.Stimmregister.EVoting.Domain.Converters;
using Voting.Stimmregister.EVoting.Domain.Enums;
using Voting.Stimmregister.EVoting.Domain.Models;
using Voting.Stimmregister.EVoting.Rest.Integration.Tests.MockData;
using Voting.Stimmregister.EVoting.Rest.Integration.Tests.Mocks;
using Voting.Stimmregister.EVoting.Rest.Models.Request;
using Voting.Stimmregister.EVoting.Rest.Models.Response;

namespace Voting.Stimmregister.EVoting.Rest.Integration.Tests.RegistrationTests;

public class RegisterTest : BaseRestTest
{
    private const string RegisterApiUrl = "v1/register";

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter(), new DateOnlyJsonConverter() },
        PropertyNameCaseInsensitive = true,
    };

    public RegisterTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    private List<UserNotification> SentUserNotifications => GetService<EmailServiceMock>().Sent;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        SentUserNotifications.Clear();
    }

    [Fact]
    public async Task ShouldWork()
    {
        var dateOfBirth = new DateOnly(1950, 01, 23);
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Unregistered,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            true,
            BfsMunicipalityMockedData.BfsAllowedForEVoting);
        HttpClientFactoryMock.StimmregisterRegisterResponse = HttpClientFactoryMock.CreateOkStimmregisterResponse();

        using var resp = await Register(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonValid, dateOfBirth);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var exists = await RunOnDb(db =>
        {
            return db.EVotingStatusChanges.AnyAsync(c =>
                c.EVotingRegistered
                && c.Active
                && c.Person!.Ahvn13 == Ahvn13MockedData.Ahvn13Valid1
                && c.Person.CantonBfs == BfsCantonMockedData.BfsCantonValid);
        });
        exists.Should().Be(true);
    }

    [Fact]
    public async Task ShouldWorkWithEmail()
    {
        var dateOfBirth = new DateOnly(1950, 01, 23);
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Unregistered,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            true,
            BfsMunicipalityMockedData.BfsAllowedWithEmail,
            cantonBfs: BfsCantonMockedData.BfsCantonEmailRequired);

        var email = "test@example.invalid";
        using var resp = await Register(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonEmailRequired, dateOfBirth, email);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await resp.Content.ReadFromJsonAsync<RegisterResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.SuccessWithPendingEmailVerification);

        var statusChangeExists = await RunOnDb(db =>
        {
            return db.EVotingStatusChanges.AnyAsync(c =>
                c.EVotingRegistered
                && c.Active
                && c.Person!.Ahvn13 == Ahvn13MockedData.Ahvn13Valid1
                && c.Person.CantonBfs == BfsCantonMockedData.BfsCantonEmailRequired);
        });
        statusChangeExists.Should().Be(false);

        var emailVerification = await RunOnDb(db => db.EmailVerifications.FirstOrDefaultAsync(x => x.Ahvn13 == Ahvn13MockedData.Ahvn13Valid1));
        emailVerification.Should().NotBeNull();
        emailVerification!.VerificationCode.Should().NotBeNullOrWhiteSpace();
        emailVerification.Email.Should().Be(email);

        SentUserNotifications.Should().HaveCount(1);
        SentUserNotifications[0].RecipientEmail.Should().Be(email);
    }

    [Fact]
    public async Task ShouldWorkWithEmailWithExistingExpiredVerification()
    {
        var email = "test@example.invalid";
        await RunOnDb(async db =>
        {
            db.EmailVerifications.Add(new EmailVerificationEntry
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1,
                Email = email,
                CreatedAt = MockedClock.UtcNowDate.AddDays(-10),
            });
            await db.SaveChangesAsync();
        });

        var dateOfBirth = new DateOnly(1950, 01, 23);
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Unregistered,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            true,
            BfsMunicipalityMockedData.BfsAllowedWithEmail);

        using var resp = await Register(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonEmailRequired, dateOfBirth, email);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await resp.Content.ReadFromJsonAsync<RegisterResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.SuccessWithPendingEmailVerification);

        var statusChangeExists = await RunOnDb(db =>
        {
            return db.EVotingStatusChanges.AnyAsync(c =>
                c.EVotingRegistered
                && c.Active
                && c.Person!.Ahvn13 == Ahvn13MockedData.Ahvn13Valid1);
        });
        statusChangeExists.Should().Be(false);

        var emailVerification = await RunOnDb(db => db.EmailVerifications.FirstOrDefaultAsync(x =>
            x.Ahvn13 == Ahvn13MockedData.Ahvn13Valid1
            && x.CreatedAt == MockedClock.UtcNowDate));
        emailVerification.Should().NotBeNull();
        emailVerification!.VerificationCode.Should().NotBeNullOrWhiteSpace();
        emailVerification.Email.Should().Be(email);

        SentUserNotifications.Should().HaveCount(1);
        SentUserNotifications[0].RecipientEmail.Should().Be(email);
    }

    [Fact]
    public async Task ShouldWorkWithExistingActiveStatusChange()
    {
        var dateOfBirth = new DateOnly(1950, 01, 23);

        await RunOnDb(async db =>
        {
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
            BfsMunicipalityMockedData.BfsAllowedForEVoting);
        HttpClientFactoryMock.StimmregisterRegisterResponse = HttpClientFactoryMock.CreateOkStimmregisterResponse();

        using var resp = await Register(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonValid, dateOfBirth);

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
        var dateOfBirth = new DateOnly(1950, 01, 23);

        await RunOnDb(async db =>
        {
            db.EVotingStatusChanges.Add(new EVotingStatusChangeEntity
            {
                EVotingRegistered = false,
                Active = false,
                ContextId = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
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
            BfsMunicipalityMockedData.BfsAllowedForEVoting);
        HttpClientFactoryMock.StimmregisterRegisterResponse = HttpClientFactoryMock.CreateOkStimmregisterResponse();

        using var resp = await Register(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonValid, dateOfBirth);

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
    public async Task ShouldReturnErrorWhenNotAllowedToVote()
    {
        var dateOfBirth = new DateOnly(1950, 01, 23);
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Unknown,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            false,
            BfsMunicipalityMockedData.BfsAllowedForEVoting);

        using var resp = await Register(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonValid, dateOfBirth);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await resp.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.EVotingPermissionError);
    }

    [Fact]
    public async Task ShouldReturnErrorWhenTooYoung()
    {
        var dateOfBirth = new DateOnly(2023, 01, 23);
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Unknown,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            true,
            BfsMunicipalityMockedData.BfsAllowedWithEmail);

        using var resp = await Register(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonEmailRequired, dateOfBirth, "test@example.invalid");

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await resp.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.EVotingPermissionError);
    }

    [Fact]
    public async Task ShouldReturnErrorWhenEmailMissing()
    {
        var dateOfBirth = new DateOnly(1988, 01, 23);
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Unknown,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            true,
            BfsMunicipalityMockedData.BfsAllowedWithEmail);

        using var resp = await Register(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonEmailRequired, dateOfBirth);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await resp.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.InvalidEmailFormat);
    }

    [Fact]
    public async Task ShouldReturnErrorWhenAlreadyRegistered()
    {
        var dateOfBirth = new DateOnly(1950, 01, 23);
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Registered,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            true,
            BfsMunicipalityMockedData.BfsAllowedForEVoting);

        using var resp = await Register(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonValid, dateOfBirth);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await resp.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.EVotingAlreadyRegistered);
    }

    [Fact]
    public async Task ShouldDeleteExistingPendingEmailVerification()
    {
        var dateOfBirth = new DateOnly(1950, 01, 23);
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Unregistered,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            true,
            BfsMunicipalityMockedData.BfsAllowedWithEmail);

        var email = "test@example.invalid";
        var oldVerificationCode = "test123";
        await RunOnDb(async db =>
        {
            db.EmailVerifications.Add(new EmailVerificationEntry
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1,
                Email = email,
                CreatedAt = MockedClock.UtcNowDate,
                VerificationCode = oldVerificationCode,
            });
            await db.SaveChangesAsync();
        });

        using var resp = await Register(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonEmailRequired, dateOfBirth, email);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await resp.Content.ReadFromJsonAsync<RegisterResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.SuccessWithPendingEmailVerification);

        var newVerification = await RunOnDb(db => db.EmailVerifications.SingleAsync(v => v.Ahvn13 == Ahvn13MockedData.Ahvn13Valid1));
        newVerification.VerificationCode.Should().NotBe(oldVerificationCode);
    }

    [Fact]
    public async Task ShouldReturnErrorWhenDateOfBirthDoesNotMatch()
    {
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Unregistered,
            Ahvn13MockedData.Ahvn13Valid1,
            new DateOnly(1970, 12, 25),
            true,
            BfsMunicipalityMockedData.BfsAllowedForEVoting);

        using var resp = await Register(
            Ahvn13MockedData.Ahvn13Valid1Formatted,
            BfsCantonMockedData.BfsCantonValid,
            new DateOnly(1950, 01, 23));

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await resp.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.DateOfBirthDoesNotMatch);
    }

    [Fact]
    public async Task ShouldReturnErrorResponseWhenStimmregisterReturnsErrorCode()
    {
        var dateOfBirth = new DateOnly(1950, 01, 23);
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Unregistered,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            true,
            BfsMunicipalityMockedData.BfsAllowedForEVoting);
        HttpClientFactoryMock.StimmregisterRegisterResponse =
            HttpClientFactoryMock.CreateErrorResponse(HttpStatusCode.BadRequest, ProcessStatusCode.EVotingPermissionError);

        using var resp = await Register(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonValid, dateOfBirth);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await resp.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.EVotingPermissionError);
    }

    [Fact]
    public async Task ShouldReturnErrorWhenCantonLimitReached()
    {
        var dateOfBirth = new DateOnly(1950, 01, 23);
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Unregistered,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            true,
            BfsMunicipalityMockedData.BfsAllowedForEVoting,
            "Schweiz",
            900);

        using var resp = await Register(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonValid, dateOfBirth);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await resp.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.EVotingReachedMaxAllowedVoters);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenPassingEmptyAhvn13()
    {
        await AssertStatus(
            () => Register(string.Empty, BfsCantonMockedData.BfsCantonValid, new DateOnly(1950, 01, 23)),
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenPassingInvalidAhvn13Checksum()
    {
        await AssertStatus(
            () => Register(Ahvn13MockedData.Ahvn13InvalidChecksumFormatted, BfsCantonMockedData.BfsCantonValid, new DateOnly(1950, 01, 23)),
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenPassingNoContent()
    {
        await AssertStatus(
            () => AdminClient.PostAsJsonAsync(RegisterApiUrl, new { }),
            HttpStatusCode.BadRequest);
    }

    protected override Task<HttpResponseMessage> AuthorizationTestCall(HttpClient httpClient)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, RegisterApiUrl);
        request.Content = JsonContent.Create(
            new RegisterRequest
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1Formatted,
                BfsCanton = BfsCantonMockedData.BfsCantonValid,
                DateOfBirth = new DateOnly(1950, 01, 23),
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

    private async Task<HttpResponseMessage> Register(string ahvn13, short bfsCanton, DateOnly dateOfBirth, string? email = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, RegisterApiUrl);
        request.Content = JsonContent.Create(
            new RegisterRequest
            {
                Ahvn13 = ahvn13,
                BfsCanton = bfsCanton,
                DateOfBirth = dateOfBirth,
                Email = email,
            },
            options: _jsonOptions);
        request.Headers.Add("X-Context-Id", Guid.NewGuid().ToString());
        return await AdminClient.SendAsync(request);
    }
}
