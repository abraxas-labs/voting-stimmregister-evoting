// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmregister.EVoting.Adapter.DokConnector.Configuration;

public class DokConnectorConfig : Lib.DokConnector.Configuration.DokConnectorConfig
{
#if DEBUG
    public bool EnableMock { get; set; }
#endif
}
