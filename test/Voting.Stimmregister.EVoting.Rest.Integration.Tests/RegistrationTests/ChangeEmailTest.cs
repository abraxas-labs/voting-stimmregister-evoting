// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
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

public class ChangeEmailTest : BaseRestTest
{
    private const string ChangeEmailApiUrl = "v1/change-email";

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter(), new DateOnlyJsonConverter() },
        PropertyNameCaseInsensitive = true,
    };

    public ChangeEmailTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ShouldWork()
    {
        var dateOfBirth = new DateOnly(1950, 01, 23);
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Registered,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            true,
            BfsMunicipalityMockedData.BfsAllowedWithEmail);
        HttpClientFactoryMock.StimmregisterUnregisterResponse = HttpClientFactoryMock.CreateOkStimmregisterResponse();

        using var resp = await ChangeEmail(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonEmailRequired, dateOfBirth, "test@domain.invalid");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var hasStatusChange = await RunOnDb(db => db.EVotingStatusChanges.AnyAsync());
        hasStatusChange.Should().Be(false);

        var verification = await RunOnDb(db => db.EmailVerifications.SingleAsync(v => v.Ahvn13 == Ahvn13MockedData.Ahvn13Valid1));
        verification.IsEmailChange.Should().Be(true);
        verification.VerificationCode.Should().NotBeNullOrWhiteSpace();
        verification.Email.Should().Be("test@domain.invalid");
    }

    [Fact]
    public async Task ShouldWorkWhenEmailVerificationPending()
    {
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateErrorResponse(HttpStatusCode.BadRequest, ProcessStatusCode.EmailCannotBeChanged);
        HttpClientFactoryMock.StimmregisterUnregisterResponse = HttpClientFactoryMock.CreateErrorResponse(HttpStatusCode.BadRequest, ProcessStatusCode.EmailCannotBeChanged);

        var dateOfBirth = new DateOnly(1950, 01, 23);
        var email = "test@domain.invalid";
        await RunOnDb(async db =>
        {
            db.EmailVerifications.Add(new EmailVerificationEntry
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1,
                Email = email,
                VerificationCode = "test234",
                BfsCanton = BfsCantonMockedData.BfsCantonEmailRequired,
                CreatedAt = MockedClock.UtcNowDate,
                DateOfBirth = dateOfBirth,
            });
            await db.SaveChangesAsync();
        });

        using var resp = await ChangeEmail(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonEmailRequired, dateOfBirth, email);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var hasStatusChange = await RunOnDb(db => db.EVotingStatusChanges.AnyAsync());
        hasStatusChange.Should().Be(false);

        var verification = await RunOnDb(db => db.EmailVerifications.SingleAsync(v => v.Ahvn13 == Ahvn13MockedData.Ahvn13Valid1));
        verification.IsEmailChange.Should().Be(false);
        verification.VerificationCode.Should().NotBeNullOrWhiteSpace();
        verification.VerificationCode.Should().NotBe("test234");
        verification.Email.Should().Be("test@domain.invalid");
    }

    [Fact]
    public async Task ShouldWorkWhenEmailChangePending()
    {
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateErrorResponse(HttpStatusCode.BadRequest, ProcessStatusCode.EmailCannotBeChanged);
        HttpClientFactoryMock.StimmregisterUnregisterResponse = HttpClientFactoryMock.CreateErrorResponse(HttpStatusCode.BadRequest, ProcessStatusCode.EmailCannotBeChanged);

        var dateOfBirth = new DateOnly(1950, 01, 23);
        var email = "test@domain.invalid";
        await RunOnDb(async db =>
        {
            db.EmailVerifications.Add(new EmailVerificationEntry
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1,
                Email = email,
                VerificationCode = "test234",
                BfsCanton = BfsCantonMockedData.BfsCantonEmailRequired,
                CreatedAt = MockedClock.UtcNowDate,
                DateOfBirth = dateOfBirth,
                IsEmailChange = true,
            });
            await db.SaveChangesAsync();
        });

        using var resp = await ChangeEmail(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonEmailRequired, dateOfBirth, email);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var hasStatusChange = await RunOnDb(db => db.EVotingStatusChanges.AnyAsync());
        hasStatusChange.Should().Be(false);

        var verification = await RunOnDb(db => db.EmailVerifications.SingleAsync(v => v.Ahvn13 == Ahvn13MockedData.Ahvn13Valid1));
        verification.IsEmailChange.Should().Be(true);
        verification.VerificationCode.Should().NotBeNullOrWhiteSpace();
        verification.VerificationCode.Should().NotBe("test234");
        verification.Email.Should().Be("test@domain.invalid");
    }

    [Fact]
    public async Task ShouldReturnErrorWhenNotRegistered()
    {
        var dateOfBirth = new DateOnly(1950, 01, 23);
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Unregistered,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            true,
            BfsMunicipalityMockedData.BfsAllowedWithEmail);

        using var resp = await ChangeEmail(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonEmailRequired, dateOfBirth, "test@test.invalid");

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await resp.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.EVotingAlreadyUnregisteredOrUnknown);
    }

    [Fact]
    public async Task ShouldReturnErrorWhenDateOfBirthDoesNotMatch()
    {
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Registered,
            Ahvn13MockedData.Ahvn13Valid1,
            new DateOnly(1970, 12, 25),
            true,
            BfsMunicipalityMockedData.BfsAllowedForEVoting);

        using var resp = await ChangeEmail(
            Ahvn13MockedData.Ahvn13Valid1Formatted,
            BfsCantonMockedData.BfsCantonEmailRequired,
            new DateOnly(1950, 01, 23),
            "test@test.invalid");

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await resp.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.DateOfBirthDoesNotMatch);
    }

    [Fact]
    public async Task ShouldReturnErrorWhenEmailChangeRateLimitReached()
    {
        var dateOfBirth = new DateOnly(1950, 01, 23);
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Registered,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            true,
            BfsMunicipalityMockedData.BfsAllowedWithEmail);
        HttpClientFactoryMock.StimmregisterUnregisterResponse = HttpClientFactoryMock.CreateOkStimmregisterResponse();

        using var resp = await ChangeEmail(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonEmailRequired, dateOfBirth, "test@domain.invalid");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        using var resp2 = await ChangeEmail(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonEmailRequired, dateOfBirth, "test@domain.invalid");
        resp2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await resp2.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.EmailChangeRateLimitExceeded);
    }

    [Fact]
    public async Task ShouldReturnErrorResponseWhenStimmregisterReturnsErrorCode()
    {
        var dateOfBirth = new DateOnly(1950, 01, 23);
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateErrorResponse(HttpStatusCode.BadRequest, ProcessStatusCode.EVotingPermissionError);

        using var resp = await ChangeEmail(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonEmailRequired, dateOfBirth, "test@test.invalid");

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await resp.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.EVotingPermissionError);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenPassingEmptyAhvn13()
    {
        await AssertStatus(
            () => ChangeEmail(string.Empty, BfsCantonMockedData.BfsCantonEmailRequired, new DateOnly(1950, 01, 23), "test@example.com"),
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenOnCantonWithoutEmail()
    {
        await AssertStatus(
            () => ChangeEmail(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonValid, new DateOnly(1950, 01, 23), "test@example.com"),
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenPassingEmptyEmail()
    {
        await AssertStatus(
            () => ChangeEmail(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonEmailRequired, new DateOnly(1950, 01, 23), string.Empty),
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenPassingInvalidEmail()
    {
        await AssertStatus(
            () => ChangeEmail(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonEmailRequired, new DateOnly(1950, 01, 23), "wrong-email"),
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenPassingInvalidAhvn13Checksum()
    {
        await AssertStatus(
            () => ChangeEmail(Ahvn13MockedData.Ahvn13InvalidChecksumFormatted, BfsCantonMockedData.BfsCantonEmailRequired, new DateOnly(1950, 01, 23), "test@test.invalid"),
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenPassingNoContent()
    {
        await AssertStatus(
            () => AdminClient.PostAsJsonAsync(ChangeEmailApiUrl, new { }),
            HttpStatusCode.BadRequest);
    }

    protected override Task<HttpResponseMessage> AuthorizationTestCall(HttpClient httpClient)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, ChangeEmailApiUrl);
        request.Content = JsonContent.Create(
            new ChangeEmailRequest
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1Formatted,
                BfsCanton = BfsCantonMockedData.BfsCantonValid,
                DateOfBirth = new DateOnly(1950, 01, 23),
                Email = "changed@test.invalid",
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

    private async Task<HttpResponseMessage> ChangeEmail(string ahvn13, short bfsCanton, DateOnly dateOfBirth, string email)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, ChangeEmailApiUrl);
        request.Content = JsonContent.Create(
            new ChangeEmailRequest
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
