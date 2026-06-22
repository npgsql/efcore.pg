namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class NpgsqlNetTopologySuiteApiConsistencyTest(
    NpgsqlNetTopologySuiteApiConsistencyTest.NpgsqlNetTopologySuiteApiConsistencyFixture fixture)
    : ApiConsistencyTestBase<NpgsqlNetTopologySuiteApiConsistencyTest.NpgsqlNetTopologySuiteApiConsistencyFixture>(fixture)
{
    protected override void AddServices(ServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkNpgsqlNetTopologySuite();

    protected override Assembly TargetAssembly
        => typeof(NpgsqlNetTopologySuiteServiceCollectionExtensions).Assembly;

    public class NpgsqlNetTopologySuiteApiConsistencyFixture : ApiConsistencyFixtureBase
    {
        public override HashSet<Type> FluentApiTypes { get; } =
            [typeof(NpgsqlNetTopologySuiteDbContextOptionsBuilderExtensions), typeof(NpgsqlNetTopologySuiteServiceCollectionExtensions)];
    }
}
