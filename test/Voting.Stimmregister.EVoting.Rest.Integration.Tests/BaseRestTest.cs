// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Voting.Lib.Iam.Testing.AuthenticationScheme;
using Voting.Lib.Testing;
using Voting.Stimmregister.EVoting.Adapter.Data;
using Voting.Stimmregister.EVoting.Domain.Authorization;

namespace Voting.Stimmregister.EVoting.Rest.Integration.Tests;

public abstract class BaseRestTest : RestAuthorizationBaseTest<TestApplicationFactory, TestStartup>
{
    private readonly Lazy<HttpClient> _lazyUnauthorizedClient;
    private readonly Lazy<HttpClient> _lazyAdminClient;

    protected BaseRestTest(TestApplicationFactory factory)
        : base(factory)
    {
        _lazyUnauthorizedClient = new Lazy<HttpClient>(() =>
            CreateHttpClient(false));

        _lazyAdminClient = new Lazy<HttpClient>(() =>
            CreateHttpClient(roles: new[] { Roles.Admin }, tenant: SecureConnectTestDefaults.MockedTenantDefault.Id, userId: SecureConnectTestDefaults.MockedUserService.Loginid));
    }

    protected HttpClient UnauthorizedClient => _lazyUnauthorizedClient.Value;

    protected HttpClient AdminClient => _lazyAdminClient.Value;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        await ResetDb();
    }

    protected Task ResetDb() => RunOnDb(DatabaseUtil.Truncate);

    protected Task<TResult> RunOnDb<TResult>(Func<DataContext, Task<TResult>> action)
        => RunScoped(action);

    protected Task RunOnDb(Func<DataContext, Task> action)
        => RunScoped(action);
}
