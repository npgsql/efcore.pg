using Microsoft.EntityFrameworkCore.Query.Relationships.OwnedNavigations;
using Microsoft.EntityFrameworkCore.Query.Relationships.OwnedTableSplitting;

namespace Microsoft.EntityFrameworkCore;

public class NpgsqlComplianceTest : RelationalComplianceTestBase
{
    protected override ICollection<Type> IgnoredTestBases { get; } = new HashSet<Type>
    {
        // Temporary, remove for 10.0.0-rc.2
        typeof(OwnedTableSplittingMiscellaneousRelationalTestBase<>),
        typeof(OwnedTableSplittingProjectionRelationalTestBase<>),
        typeof(OwnedNavigationsCollectionRelationalTestBase<>),
        typeof(OwnedNavigationsMiscellaneousRelationalTestBase<>),
        typeof(OwnedNavigationsProjectionRelationalTestBase<>),
        typeof(OwnedNavigationsStructuralEqualityRelationalTestBase<>),
        typeof(OwnedTableSplittingStructuralEqualityRelationalTestBase<>),

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
