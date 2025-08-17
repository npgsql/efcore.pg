namespace Microsoft.EntityFrameworkCore;

public class NpgsqlComplianceTest : RelationalComplianceTestBase
{
    protected override ICollection<Type> IgnoredTestBases { get; } = new HashSet<Type>
    {
        // TODO: Enable for rc.1 (query support for complex collections mapped to JSON)
        typeof(ComplexCollectionJsonUpdateTestBase<>),

        // Not implemented
        typeof(CompiledModelTestBase), typeof(CompiledModelRelationalTestBase), // #3087
        typeof(FromSqlSprocQueryTestBase<>),
        typeof(UdfDbFunctionTestBase<>),
        typeof(UpdateSqlGeneratorTestBase),

        // Disabled
        typeof(GraphUpdatesTestBase<>),
        typeof(ProxyGraphUpdatesTestBase<>),
        typeof(OperatorsProceduralQueryTestBase),
    };

    protected override Assembly TargetAssembly { get; } = typeof(NpgsqlComplianceTest).Assembly;
}
