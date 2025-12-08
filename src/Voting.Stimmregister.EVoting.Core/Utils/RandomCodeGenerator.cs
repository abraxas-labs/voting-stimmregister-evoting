// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System.Security.Cryptography;

namespace Voting.Stimmregister.EVoting.Core.Utils;

public static class RandomCodeGenerator
{
    // Alphanumeric characters. Confusing items such as I and l, O and 0 were removed
    private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";

    public static string CreateRandomAlphanumericString(int length)
    {
        return RandomNumberGenerator.GetString(Chars, length);
    }
}
