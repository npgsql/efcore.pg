namespace Microsoft.EntityFrameworkCore.TestUtilities;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class MinimumPostgresVersionAttribute(int major, int minor) : Attribute, ITestCondition
{
    private readonly Version _version = new(major, minor);

    public ValueTask<bool> IsMetAsync()
        => new(TestEnvironment.PostgresVersion >= _version);

    public string SkipReason
        => $"Requires PostgreSQL version {_version} or later.";
}
