using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Microsoft.EntityFrameworkCore;

public class SpatialNpgsqlFixture : SpatialFixtureBase
{
    // We instruct the test store to pass a connection string to UseNpgsql() instead of a DbConnection - that's required to allow
    // EF's UseNetTopologySuite() to function properly and instantiate an NpgsqlDataSource internally.
    protected override ITestStoreFactory TestStoreFactory
        => new NpgsqlTestStoreFactory(useConnectionString: true);

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => base.AddServices(serviceCollection)
            .AddEntityFrameworkNpgsqlNetTopologySuite();

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
    {
        var optionsBuilder = base.AddOptions(builder);
        new NpgsqlDbContextOptionsBuilder(optionsBuilder)
            .UseNetTopologySuite()
            .SetPostgresVersion(TestEnvironment.PostgresVersion);

        return optionsBuilder;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.HasPostgresExtension("uuid-ossp");
    }
}
