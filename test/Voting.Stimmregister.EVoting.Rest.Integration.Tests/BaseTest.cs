// (c) Copyright by Abraxas Informatik AG
// For license information see LICENSE file

using System;
using System.Threading.Tasks;
using Voting.Lib.Testing;
using Voting.Stimmregister.EVoting.Adapter.Data;

namespace Voting.Stimmregister.EVoting.Rest.Integration.Tests;

public abstract class BaseTest : BaseTest<TestApplicationFactory, TestStartup>
{
    protected BaseTest(TestApplicationFactory factory)
        : base(factory)
    {
    }

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
