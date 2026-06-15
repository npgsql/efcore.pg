using System.Globalization;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

/// <summary>
///     Marks tests as requiring PostGIS. When PostGIS is not available, the attribute contributes a
///     <c>category=failing</c> trait so that the test runner can exclude the affected tests.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RequiresPostgisAttribute : Attribute, global::Xunit.v3.ITraitAttribute
{
    public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits()
        => TestEnvironment.IsPostgisAvailable || Environment.GetEnvironmentVariable("NPGSQL_TEST_POSTGIS")?.ToLower(CultureInfo.InvariantCulture) is "1" or "true"
            ? []
            : [new KeyValuePair<string, string>("category", "failing")];
}
