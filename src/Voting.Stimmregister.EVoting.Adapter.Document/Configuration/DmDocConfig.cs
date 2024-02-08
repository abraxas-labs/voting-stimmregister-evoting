// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

namespace Voting.Stimmregister.EVoting.Adapter.Document.Configuration;

public class DmDocConfig : Lib.DmDoc.Configuration.DmDocConfig
{
    public string RegisteredTemplateKey { get; set; } = string.Empty;

    public string UnregisteredTemplateKey { get; set; } = string.Empty;

    public bool EnableMock { get; set; }
}
