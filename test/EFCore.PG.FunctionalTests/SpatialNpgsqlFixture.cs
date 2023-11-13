using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class SpatialNpgsqlFixture : SpatialFixtureBase
{
#pragma warning disable CS0618 // GlobalTypeMapper is obsolete
    public SpatialNpgsqlFixture()
    {
        NpgsqlConnection.GlobalTypeMapper.UseNetTopologySuite();
    }
#pragma warning restore CS0618

    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

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
