namespace Microsoft.EntityFrameworkCore.TestUtilities;

/// <summary>
///     Marks tests as requiring a minimum PostgreSQL version. When the configured PostgreSQL version
///     is older, the attribute contributes a <c>category=failing</c> trait so that the test runner can
///     exclude the affected tests.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class MinimumPostgresVersionAttribute(int major, int minor) : Attribute, global::Xunit.v3.ITraitAttribute
{
    private readonly Version _version = new(major, minor);

    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
        => TestEnvironment.PostgresVersion >= _version ? [] : [new KeyValuePair<string, string>("category", "failing")];
}
