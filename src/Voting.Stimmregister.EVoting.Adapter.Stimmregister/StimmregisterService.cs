// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Stimmregister;
using Voting.Stimmregister.EVoting.Abstractions.Core.Services;
using Voting.Stimmregister.EVoting.Adapter.Stimmregister.Mapping;
using Voting.Stimmregister.EVoting.Adapter.Stimmregister.Models;
using Voting.Stimmregister.EVoting.Domain.Converters;
using Voting.Stimmregister.EVoting.Domain.Exceptions;
using Voting.Stimmregister.EVoting.Domain.Models;
using EVotingInformation = Voting.Stimmregister.EVoting.Domain.Models.EVotingInformation;

namespace Voting.Stimmregister.EVoting.Adapter.Stimmregister;

public class StimmregisterService : IStimmregisterService
{
    private const string ContextIdHeaderKey = "X-Context-Id";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter(),
            new DateOnlyJsonConverter(),
        },
    };

    private readonly HttpClient _httpClient;
    private readonly ITracingService _tracingService;

    public StimmregisterService(HttpClient httpClient, ITracingService tracingService)
    {
        _httpClient = httpClient;
        _tracingService = tracingService;
    }

    public async Task RegisterAsync(PersonIdentification personIdentification, CancellationToken ct)
    {
        using var response = await PostToStimmregister("v2/evoting/register", personIdentification, ct);
    }

    public async Task UnregisterAsync(PersonIdentification personIdentification, CancellationToken ct)
    {
        using var response = await PostToStimmregister("v2/evoting/unregister", personIdentification, ct);
    }

    public async Task<EVotingInformation> GetEVotingInformationAsync(PersonIdentification personIdentification, CancellationToken ct)
    {
        using var response = await PostToStimmregister("v2/evoting/information", personIdentification, ct);

        var eVotingInfo = await response.Content.ReadFromJsonAsync<Models.EVotingInformation>(JsonOptions, ct)
            ?? throw new InvalidOperationException("Response JSON was null");

        return EVotingMapper.MapEVotingInfo(eVotingInfo);
    }

    private async Task<HttpResponseMessage> PostToStimmregister(string route, PersonIdentification personIdentification, CancellationToken ct)
    {
        using var requestContent = JsonContent.Create(new StimmregisterRequest
        {
            Ahvn13 = personIdentification.Ahvn13.ToString(),
            BfsCanton = personIdentification.BfsCanton,
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, route)
        {
            Content = requestContent,
        };

        if (_tracingService.ContextId != null)
        {
            request.Headers.Add(ContextIdHeaderKey, _tracingService.ContextId);
        }

        var response = await _httpClient.SendAsync(request, ct);

        if (response == null)
        {
            throw new InvalidOperationException("Response was null");
        }

        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        try
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>(JsonOptions, ct);
            throw new EVotingSubsystemException(
                $"Error calling Stimmregister: {errorResponse!.ProcessStatusMessage}",
                errorResponse.ProcessStatusCode);
        }
        catch (JsonException)
        {
            var statusCode = response.StatusCode;
            throw new EVotingSubsystemException($"Could not deserialize error response from Stimmregister. HTTP status: {statusCode}");
        }
        finally
        {
            response.Dispose();
        }
    }
}
