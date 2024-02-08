// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using Microsoft.AspNetCore.Authorization;

namespace Voting.Stimmregister.EVoting.Rest.Attributes;

/// <summary>
/// Authorize attribute for the <see cref="Voting.Stimmregister.EVoting.Domain.Authorization.Roles.Admin"/>.
/// </summary>
public class AuthorizeAdminAttribute : AuthorizeAttribute
{
    public AuthorizeAdminAttribute()
    {
        Roles = Domain.Authorization.Roles.Admin;
    }
}
