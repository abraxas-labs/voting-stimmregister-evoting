// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Voting.Stimmregister.EVoting.Adapter.Stimmregister.Models;
using Voting.Stimmregister.EVoting.Domain.Converters;
using Voting.Stimmregister.EVoting.Domain.Enums;
using Sex = Voting.Stimmregister.EVoting.Adapter.Stimmregister.Models.Sex;

namespace Voting.Stimmregister.EVoting.Rest.Integration.Tests.Mocks;

public class HttpClientFactoryMock : IHttpClientFactory
{
    public static HttpResponseMessage StimmregisterInformationResponse { get; set; } = new();

    public static HttpResponseMessage StimmregisterRegisterResponse { get; set; } = new();

    public static HttpResponseMessage StimmregisterUnregisterResponse { get; set; } = new();

    public static HttpResponseMessage CreateStimmregisterInformationResponse(
        VotingStatus votingStatus,
        long ahvn13,
        DateOnly dateOfBirth,
        bool allowedToVote,
        short municipalityBfs,
        string nationality = "Schweiz",
        int registeredEvotersInCanton = 250,
        int registeredEVotersInMunicipality = 23)
        => new()
        {
            StatusCode = HttpStatusCode.OK,
            Content = CreateJsonContent(
                new EVotingInformation
                {
                    Person = new Person
                    {
                        Ahvn13 = ahvn13,
                        DateOfBirth = dateOfBirth,
                        AllowedToVote = allowedToVote,
                        MunicipalityBfs = municipalityBfs,
                        Nationality = nationality,
                        Sex = Sex.Male,
                        FirstName = "first name",
                        OfficialName = "official name",
                        Address = new Address()
                        {
                            Street = "test street",
                            Town = "Hometown",
                            HouseNumber = "1A",
                            ZipCode = "9000",
                        },
                    },
                    VotingStatus = votingStatus,
                    RegisteredEVotersInCanton = registeredEvotersInCanton,
                    RegisteredEVotersInMunicipality = registeredEVotersInMunicipality,
                }),
        };

    public static HttpResponseMessage CreateOkStimmregisterResponse()
        => new() { StatusCode = HttpStatusCode.OK };

    public static HttpResponseMessage CreateErrorResponse(HttpStatusCode statusCode, ProcessStatusCode processStatusCode)
        => new()
        {
            StatusCode = statusCode,
            Content = CreateJsonContent(
                new ErrorResponse
                {
                    ProcessStatusCode = processStatusCode,
                    ProcessStatusMessage = "error happened",
                }),
        };

    public HttpClient CreateClient(string name)
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        // Stimmregister information
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.PathAndQuery.Contains("/v2/evoting/information")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(StimmregisterInformationResponse);

        // Stimmregister register
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.PathAndQuery.Contains("/v2/evoting/register")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(StimmregisterRegisterResponse);

        // Stimmregister unregister
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri != null && r.RequestUri.PathAndQuery.Contains("/v2/evoting/unregister")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(StimmregisterUnregisterResponse);

        return new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://test-domain.invalid"), };
    }

    private static HttpContent CreateJsonContent<T>(T value)
    {
        return JsonContent.Create(value, options: new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new DateOnlyJsonConverter() },
        });
    }
}
