namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class NpgsqlNetTopologySuiteApiConsistencyTest : ApiConsistencyTestBase<
    NpgsqlNetTopologySuiteApiConsistencyTest.NpgsqlNetTopologySuiteApiConsistencyFixture>
{
    public NpgsqlNetTopologySuiteApiConsistencyTest(NpgsqlNetTopologySuiteApiConsistencyFixture fixture)
        : base(fixture)
    {
    }

    protected override void AddServices(ServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkNpgsqlNetTopologySuite();

    protected override Assembly TargetAssembly
        => typeof(NpgsqlNetTopologySuiteServiceCollectionExtensions).Assembly;

    public class NpgsqlNetTopologySuiteApiConsistencyFixture : ApiConsistencyFixtureBase
    {
        public override HashSet<Type> FluentApiTypes { get; } = new()
        {
            typeof(NpgsqlNetTopologySuiteDbContextOptionsBuilderExtensions), typeof(NpgsqlNetTopologySuiteServiceCollectionExtensions)
        };
    }
}
