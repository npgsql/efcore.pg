namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class MinimumPostgresVersionAttribute : Attribute, ITestCondition
{
    private readonly Version _version;

    public MinimumPostgresVersionAttribute(int major, int minor)
    {
        _version = new Version(major, minor);
    }

    public ValueTask<bool> IsMetAsync()
        => new(TestEnvironment.PostgresVersion >= _version);

    public string SkipReason
        => $"Requires PostgreSQL version {_version} or later.";
}
