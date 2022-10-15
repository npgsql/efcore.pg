namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class NpgsqlComplianceTest : RelationalComplianceTestBase
{
    protected override ICollection<Type> IgnoredTestBases { get; } = new HashSet<Type>
    {
        // Not implemented
        typeof(StoredProcedureUpdateTestBase),
        typeof(JsonUpdateTestBase<>),
        typeof(JsonQueryTestBase<>),
        typeof(JsonQueryAdHocTestBase),

        // Not implemented
        typeof(FromSqlSprocQueryTestBase<>),
        typeof(UdfDbFunctionTestBase<>),
        typeof(UpdateSqlGeneratorTestBase),

        // Disabled
        typeof(GraphUpdatesTestBase<>),
        typeof(ProxyGraphUpdatesTestBase<>)
    };

    protected override Assembly TargetAssembly { get; } = typeof(NpgsqlComplianceTest).Assembly;
}
