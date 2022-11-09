using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class SpatialQueryNpgsqlFixture : SpatialQueryRelationalFixture
{
#pragma warning disable CS0618 // GlobalTypeMapper is obsolete
    public SpatialQueryNpgsqlFixture()
    {
        NpgsqlConnection.GlobalTypeMapper.UseNetTopologySuite();
    }
#pragma warning restore CS0618

    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => base.AddServices(serviceCollection).AddEntityFrameworkNpgsqlNetTopologySuite();

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.HasPostgresExtension("postgis");
    }

    // TODO: #1232
    // protected override bool CanExecuteQueryString => true;
}
