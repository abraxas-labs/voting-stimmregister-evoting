// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Security.Cryptography;

namespace Voting.Stimmregister.EVoting.Core.Utils;

public static class RandomCodeGenerator
{
    /// <summary>
    /// Generates a random code of length <paramref name="length"/> and normalizes as
    /// Base64 encoded string with a URL and filename safe alphabet, as specified in
    /// <see href="https://datatracker.ietf.org/doc/html/rfc4648#section-5">RFC 4648, Section 5</see>.
    /// The resulting string is suitable for use in URLs and filenames.
    /// </summary>
    /// <param name="length">
    /// The length of the code to generate.
    /// </param>
    /// <returns>
    /// The normalized URL-safe Base64 encoded format of the generated code.
    /// The length of the normalized code is calculated as: 4 * ceil(length / 3) - num_of_paddings.
    /// </returns>
    public static string GenerateBase64UrlSafeCode(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);

        Span<byte> buffer = stackalloc byte[length];
        RandomNumberGenerator.Fill(buffer);

        var base64 = Convert.ToBase64String(buffer);

        return base64
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
