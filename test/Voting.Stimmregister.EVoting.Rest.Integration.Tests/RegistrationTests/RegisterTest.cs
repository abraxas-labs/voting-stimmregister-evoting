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
using Voting.Stimmregister.EVoting.Adapter.Stimmregister.Models;
using Voting.Stimmregister.EVoting.Domain.Converters;
using Voting.Stimmregister.EVoting.Domain.Enums;
using Voting.Stimmregister.EVoting.Domain.Models;
using Voting.Stimmregister.EVoting.Rest.Integration.Tests.MockData;
using Voting.Stimmregister.EVoting.Rest.Integration.Tests.Mocks;
using Voting.Stimmregister.EVoting.Rest.Models.Request;

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
                && c.Person!.Ahvn13 == Ahvn13MockedData.Ahvn13Valid1);
        });
        exists.Should().Be(true);
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
            BfsMunicipalityMockedData.BfsAllowedForEVoting);

        using var resp = await Register(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonValid, dateOfBirth);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await resp.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.EVotingPermissionError);
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
            HttpClientFactoryMock.CreateErrorResponse(HttpStatusCode.BadRequest, ProcessStatusCode.LogantoServiceRequestError);

        using var resp = await Register(Ahvn13MockedData.Ahvn13Valid1Formatted, BfsCantonMockedData.BfsCantonValid, dateOfBirth);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await resp.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.LogantoServiceRequestError);
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
        using var request = new HttpRequestMessage(HttpMethod.Post, RegisterApiUrl);
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

    private async Task<HttpResponseMessage> Register(string ahvn13, short bfsCanton, DateOnly dateOfBirth)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, RegisterApiUrl);
        request.Content = JsonContent.Create(
            new RegisterRequest
            {
                Ahvn13 = ahvn13,
                BfsCanton = bfsCanton,
                DateOfBirth = dateOfBirth,
            },
            options: _jsonOptions);
        request.Headers.Add("X-Context-Id", Guid.NewGuid().ToString());
        return await AdminClient.SendAsync(request);
    }
}
