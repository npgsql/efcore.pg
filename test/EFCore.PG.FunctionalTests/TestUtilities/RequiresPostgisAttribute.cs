namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RequiresPostgisAttribute : Attribute, ITestCondition
{
    public ValueTask<bool> IsMetAsync()
        => new(TestEnvironment.IsPostgisAvailable);

    public string SkipReason
        => "Requires PostGIS";
}
