// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Microsoft.Extensions.DependencyInjection;
using Voting.Stimmregister.EVoting.Abstractions.Core.Services;
using Voting.Stimmregister.EVoting.Domain.Configuration;

namespace Voting.Stimmregister.EVoting.Core.Services;

public class EVoterServiceFactory
{
    private readonly EVotingConfig _evotingConfig;
    private readonly IServiceProvider _serviceProvider;
    private readonly ObjectFactory<EVoterService> _eVoterServiceFactory
        = ActivatorUtilities.CreateFactory<EVoterService>([typeof(EVotingCustomConfig), typeof(short)]);

    public EVoterServiceFactory(EVotingConfig evotingConfig, IServiceProvider serviceProvider)
    {
        _evotingConfig = evotingConfig;
        _serviceProvider = serviceProvider;
    }

    public IEVoterService CreateEVoterService(short cantonBfs)
    {
        var bfsAsString = cantonBfs.ToString();
        if (!_evotingConfig.CustomSettings.TryGetValue(bfsAsString, out var config))
        {
            throw new InvalidOperationException($"Für den Kunden mit BFS {bfsAsString} sind keine Custom Settings verfügbar.");
        }

        return _eVoterServiceFactory(_serviceProvider, [config, cantonBfs]);
    }
}
