// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Voting.Lib.Common;

namespace Voting.Stimmregister.EVoting.Rest.Integration.Tests.MockData;

public static class Ahvn13MockedData
{
    public const long Ahvn13Valid1 = 756_8731_7841_09;
    public const long Ahvn13Valid2 = 756_9111_5026_10;
    public const long Ahvn13Valid3 = 756_6523_5720_40;
    public const long Ahvn13InvalidChecksum = 756_8731_7841_08;

    public static readonly string Ahvn13Valid1Formatted = Ahvn13.Parse(Ahvn13Valid1).ToString();
    public static readonly string Ahvn13Valid2Formatted = Ahvn13.Parse(Ahvn13Valid2).ToString();
    public static readonly string Ahvn13Valid3Formatted = Ahvn13.Parse(Ahvn13Valid3).ToString();
    public static readonly string Ahvn13InvalidChecksumFormatted = "756.8731.7841.08";
}
