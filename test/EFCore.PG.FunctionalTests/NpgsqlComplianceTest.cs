namespace Microsoft.EntityFrameworkCore;

public class NpgsqlComplianceTest : RelationalComplianceTestBase
{
    protected override ICollection<Type> IgnoredTestBases { get; } = new HashSet<Type>
    {
        // Not implemented
        typeof(CompiledModelTestBase), typeof(CompiledModelRelationalTestBase), // #3087
        typeof(FromSqlSprocQueryTestBase<>),
        typeof(UdfDbFunctionTestBase<>),
        typeof(UpdateSqlGeneratorTestBase),

        // Disabled
        typeof(GraphUpdatesTestBase<>),
        typeof(ProxyGraphUpdatesTestBase<>),
        typeof(OperatorsProceduralQueryTestBase),

        // New in EF Core 11, TODO: implement
        typeof(Query.Inheritance.InheritanceComplexTypesQueryTestBase<>),
        typeof(RuntimeMigrationTestBase<>),
        typeof(Query.AdHocComplexTypeQueryRelationalTestBase),
        typeof(Query.Inheritance.TPCInheritanceJsonQueryRelationalTestBase<>),
        typeof(Query.Inheritance.TPCInheritanceTableSplittingQueryRelationalTestBase<>),
        typeof(Query.Inheritance.TPHInheritanceJsonQueryRelationalTestBase<>),
        typeof(Query.Inheritance.TPHInheritanceTableSplittingQueryRelationalTestBase<>),
        typeof(Query.Inheritance.TPTInheritanceJsonQueryRelationalTestBase<>),
        typeof(Query.Inheritance.TPTInheritanceTableSplittingQueryRelationalTestBase<>),
    };

    protected override Assembly TargetAssembly { get; } = typeof(NpgsqlComplianceTest).Assembly;
}
