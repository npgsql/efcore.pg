namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class NpgsqlNodaTimeApiConsistencyTest : ApiConsistencyTestBase<NpgsqlNodaTimeApiConsistencyTest.NpgsqlNodaTimeApiConsistencyFixture>
{
    public NpgsqlNodaTimeApiConsistencyTest(NpgsqlNodaTimeApiConsistencyFixture fixture)
        : base(fixture)
    {
    }

    protected override void AddServices(ServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkNpgsqlNodaTime();

    protected override Assembly TargetAssembly
        => typeof(NpgsqlNodaTimeServiceCollectionExtensions).Assembly;

    public class NpgsqlNodaTimeApiConsistencyFixture : ApiConsistencyFixtureBase
    {
        public override HashSet<Type> FluentApiTypes { get; } = new()
        {
            typeof(NpgsqlNodaTimeDbContextOptionsBuilderExtensions), typeof(NpgsqlNodaTimeServiceCollectionExtensions)
        };
    }
}
