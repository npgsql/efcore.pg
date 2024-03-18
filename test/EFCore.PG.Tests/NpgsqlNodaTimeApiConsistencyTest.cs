namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class NpgsqlNodaTimeApiConsistencyTest(NpgsqlNodaTimeApiConsistencyTest.NpgsqlNodaTimeApiConsistencyFixture fixture)
    : ApiConsistencyTestBase<NpgsqlNodaTimeApiConsistencyTest.NpgsqlNodaTimeApiConsistencyFixture>(fixture)
{
    protected override void AddServices(ServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkNpgsqlNodaTime();

    protected override Assembly TargetAssembly
        => typeof(NpgsqlNodaTimeServiceCollectionExtensions).Assembly;

    public class NpgsqlNodaTimeApiConsistencyFixture : ApiConsistencyFixtureBase
    {
        public override HashSet<Type> FluentApiTypes { get; } =
            [typeof(NpgsqlNodaTimeDbContextOptionsBuilderExtensions), typeof(NpgsqlNodaTimeServiceCollectionExtensions)];
    }
}
