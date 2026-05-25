namespace Microsoft.EntityFrameworkCore.TestUtilities;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class MinimumPostgresVersionAttribute(int major, int minor) : Attribute, global::Xunit.v3.ITraitAttribute
{
    private readonly Version _version = new(major, minor);

    private bool IsMet
        => TestEnvironment.PostgresVersion >= _version;

    public string SkipReason
        => $"Requires PostgreSQL version {_version} or later.";

    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
        => IsMet ? [] : [new("category", "failing")];
}
