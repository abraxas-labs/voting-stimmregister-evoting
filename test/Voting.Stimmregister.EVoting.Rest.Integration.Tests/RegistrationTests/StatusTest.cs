// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Voting.Stimmregister.EVoting.Adapter.Stimmregister.Models;
using Voting.Stimmregister.EVoting.Domain.Converters;
using Voting.Stimmregister.EVoting.Domain.Enums;
using Voting.Stimmregister.EVoting.Rest.Integration.Tests.MockData;
using Voting.Stimmregister.EVoting.Rest.Integration.Tests.Mocks;
using Voting.Stimmregister.EVoting.Rest.Models.Request;
using Voting.Stimmregister.EVoting.Rest.Models.Response;

namespace Voting.Stimmregister.EVoting.Rest.Integration.Tests.RegistrationTests;

public class StatusTest : BaseRestTest
{
    private const string StatusApiUrl = "v1/status";

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter(), new DateOnlyJsonConverter() },
        PropertyNameCaseInsensitive = true,
    };

    public StatusTest(TestApplicationFactory factory)
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
            BfsMunicipalityMockedData.BfsAllowedForEVoting);

        using var resp = await AdminClient.PostAsJsonAsync(
            StatusApiUrl,
            new GetStatusRequest
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1Formatted,
                BfsCanton = BfsCantonMockedData.BfsCantonValid,
                DateOfBirth = dateOfBirth,
            },
            _jsonOptions);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await resp.Content.ReadFromJsonAsync<GetStatusResponse>(_jsonOptions);
        content!.VotingStatus.Should().Be(EVotingStatus.Registered);
        content.VotingRight.Should().Be(VotingRight.Permitted);
    }

    [Fact]
    public async Task ShouldReturnErrorResponseWhenMunicipalityIsNotEnabledForEVoting()
    {
        var dateOfBirth = new DateOnly(1950, 01, 23);
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Unknown,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            true,
            BfsMunicipalityMockedData.BfsNotAllowedForEVoting);

        using var resp = await AdminClient.PostAsJsonAsync(
            StatusApiUrl,
            new GetStatusRequest
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1Formatted,
                BfsCanton = BfsCantonMockedData.BfsCantonValid,
                DateOfBirth = dateOfBirth,
            },
            _jsonOptions);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await resp.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.EVotingNotEnabledError);
    }

    [Fact]
    public async Task ShouldReturnErrorResponseWhenStimmregisterServiceReturnsError()
    {
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateErrorResponse(
            HttpStatusCode.InternalServerError,
            ProcessStatusCode.KewrServiceRequestError);

        using var resp = await AdminClient.PostAsJsonAsync(
            StatusApiUrl,
            new GetStatusRequest
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1Formatted,
                BfsCanton = BfsCantonMockedData.BfsCantonValid,
                DateOfBirth = new DateOnly(1950, 01, 23),
            },
            _jsonOptions);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await resp.Content.ReadFromJsonAsync<ErrorResponse>(_jsonOptions);
        content!.ProcessStatusCode.Should().Be(ProcessStatusCode.KewrServiceRequestError);
    }

    [Fact]
    public async Task ShouldReturnNotPermittedResponseWhenNotPermittedForVoting()
    {
        var dateOfBirth = new DateOnly(1950, 01, 23);
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Unknown,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            false,
            BfsMunicipalityMockedData.BfsAllowedForEVoting);

        using var resp = await AdminClient.PostAsJsonAsync(
            StatusApiUrl,
            new GetStatusRequest
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1Formatted,
                BfsCanton = BfsCantonMockedData.BfsCantonValid,
                DateOfBirth = dateOfBirth,
            },
            _jsonOptions);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await resp.Content.ReadFromJsonAsync<GetStatusResponse>(_jsonOptions);
        content!.VotingStatus.Should().Be(EVotingStatus.Unknown);
        content.VotingRight.Should().Be(VotingRight.NotPermitted);
    }

    [Fact]
    public async Task ShouldReturnNotPermittedResponseWhenTooYoungForVoting()
    {
        var dateOfBirth = new DateOnly(2023, 01, 23);
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Unknown,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            true,
            BfsMunicipalityMockedData.BfsAllowedForEVoting);

        using var resp = await AdminClient.PostAsJsonAsync(
            StatusApiUrl,
            new GetStatusRequest
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1Formatted,
                BfsCanton = BfsCantonMockedData.BfsCantonValid,
                DateOfBirth = dateOfBirth,
            },
            _jsonOptions);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await resp.Content.ReadFromJsonAsync<GetStatusResponse>(_jsonOptions);
        content!.VotingStatus.Should().Be(EVotingStatus.Unknown);
        content.VotingRight.Should().Be(VotingRight.NotPermitted);
    }

    [Fact]
    public async Task ShouldReturnNotPermittedResponseWhenHasNoSwissNationality()
    {
        var dateOfBirth = new DateOnly(1950, 01, 23);
        HttpClientFactoryMock.StimmregisterInformationResponse = HttpClientFactoryMock.CreateStimmregisterInformationResponse(
            VotingStatus.Unknown,
            Ahvn13MockedData.Ahvn13Valid1,
            dateOfBirth,
            true,
            BfsMunicipalityMockedData.BfsAllowedForEVoting,
            "French");

        using var resp = await AdminClient.PostAsJsonAsync(
            StatusApiUrl,
            new GetStatusRequest
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1Formatted,
                BfsCanton = BfsCantonMockedData.BfsCantonValid,
                DateOfBirth = dateOfBirth,
            },
            _jsonOptions);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await resp.Content.ReadFromJsonAsync<GetStatusResponse>(_jsonOptions);
        content!.VotingStatus.Should().Be(EVotingStatus.Unknown);
        content.VotingRight.Should().Be(VotingRight.NotPermitted);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenNotPassingAhvn13()
    {
        await AssertStatus(
            () => AdminClient.PostAsJsonAsync(
                StatusApiUrl,
                new GetStatusRequest
                {
                    BfsCanton = BfsCantonMockedData.BfsCantonValid,
                    DateOfBirth = new DateOnly(1995, 01, 01),
                },
                _jsonOptions),
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenPassingInvalidAhvn13Checksum()
    {
        await AssertStatus(
            () => AdminClient.PostAsJsonAsync(
                StatusApiUrl,
                new GetStatusRequest
                {
                    Ahvn13 = Ahvn13MockedData.Ahvn13InvalidChecksumFormatted,
                    BfsCanton = BfsCantonMockedData.BfsCantonValid,
                    DateOfBirth = new DateOnly(1995, 01, 01),
                },
                _jsonOptions),
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenPassingEmptyAhvn13()
    {
        await AssertStatus(
            () => AdminClient.PostAsJsonAsync(
                StatusApiUrl,
                new GetStatusRequest
                {
                    Ahvn13 = string.Empty,
                    BfsCanton = BfsCantonMockedData.BfsCantonValid,
                    DateOfBirth = new DateOnly(1995, 01, 01),
                },
                _jsonOptions),
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ShouldReturnBadRequestWhenPassingInvalidDateOfBirth()
    {
        var json = "{" +
            $"\"Ahvn13\": \"{Ahvn13MockedData.Ahvn13Valid1Formatted}\"," +
            $"\"BfsCanton\": {BfsCantonMockedData.BfsCantonValid}," +
            "\"DateOfBirth\": \"invalid\"" +
            "}";
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        await AssertStatus(
            () => AdminClient.PostAsync(StatusApiUrl, content),
            HttpStatusCode.BadRequest);
    }

    protected override Task<HttpResponseMessage> AuthorizationTestCall(HttpClient httpClient)
    {
        return httpClient.PostAsJsonAsync(
            StatusApiUrl,
            new GetStatusRequest
            {
                Ahvn13 = Ahvn13MockedData.Ahvn13Valid1Formatted,
                BfsCanton = BfsCantonMockedData.BfsCantonValid,
                DateOfBirth = new DateOnly(1995, 01, 01),
            },
            _jsonOptions);
    }

    protected override IEnumerable<string> UnauthorizedRoles()
    {
        yield return NoRole;
        yield return "unkown";
    }
}
