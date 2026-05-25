using System.Globalization;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RequiresPostgisAttribute : Attribute, global::Xunit.v3.ITraitAttribute
{
    private static bool IsMet
        => TestEnvironment.IsPostgisAvailable
            || Environment.GetEnvironmentVariable("NPGSQL_TEST_POSTGIS")?.ToLower(CultureInfo.InvariantCulture) is "1" or "true";

    public string SkipReason
        => "PostGIS isn't installed, skipping";

    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
        => IsMet ? [] : [new("category", "failing")];
}
