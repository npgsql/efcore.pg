using System.Globalization;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RequiresPostgisAttribute : Attribute, global::Xunit.v3.ITraitAttribute
{
    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
        => TestEnvironment.IsPostgisAvailable || Environment.GetEnvironmentVariable("NPGSQL_TEST_POSTGIS")?.ToLower(CultureInfo.InvariantCulture) is "1" or "true"
            ? []
            : [new("category", "failing")];

    public string SkipReason
        => "PostGIS isn't installed, skipping";
}
