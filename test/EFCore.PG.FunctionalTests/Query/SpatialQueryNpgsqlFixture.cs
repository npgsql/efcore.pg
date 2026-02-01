namespace Microsoft.EntityFrameworkCore.Query;

public class SpatialQueryNpgsqlFixture : SpatialQueryRelationalFixture
{
    // We instruct the test store to pass a connection string to UseNpgsql() instead of a DbConnection - that's required to allow
    // EF's UseNodaTime() to function properly and instantiate an NpgsqlDataSource internally.
    protected override ITestStoreFactory TestStoreFactory
        => new NpgsqlTestStoreFactory(useConnectionString: true);

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
