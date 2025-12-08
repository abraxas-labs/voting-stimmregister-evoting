// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Voting.Lib.DmDoc.Serialization.Xml;
using Voting.Stimmregister.EVoting.Abstractions.Adapter.Document;

namespace Voting.Stimmregister.EVoting.Rest.Integration.Tests.Mocks;

public class PdfServiceMock : IPdfService
{
    public List<string> Generated { get; } = [];

    public Task<Stream> RenderPdf<T>(string templateKey, T data, CancellationToken ct)
    {
        var bytes = Encoding.UTF8.GetBytes(DmDocXmlSerializer.Serialize(data));
        Stream stream = new MemoryStream(bytes);
        Generated.Add(templateKey);
        return Task.FromResult(stream);
    }
}
