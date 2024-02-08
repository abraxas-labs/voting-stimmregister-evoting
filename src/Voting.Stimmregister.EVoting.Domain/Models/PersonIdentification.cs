// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using Voting.Lib.Common;

namespace Voting.Stimmregister.EVoting.Domain.Models;

public record PersonIdentification(Ahvn13 Ahvn13, short BfsCanton, DateOnly DateOfBirth)
{
}
