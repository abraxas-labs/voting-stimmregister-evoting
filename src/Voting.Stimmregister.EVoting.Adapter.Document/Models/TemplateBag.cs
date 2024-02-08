// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Xml.Serialization;

namespace Voting.Stimmregister.EVoting.Adapter.Document.Models;

[XmlRoot("Voting")]
public class TemplateBag
{
    public EVotingInformation? EVotingInformation { get; set; }
}
