namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

public class NpgsqlTestStoreFactory : RelationalTestStoreFactory
{
    private readonly string _connectionStringOptions;

    public static NpgsqlTestStoreFactory Instance { get; } = new();

    public static NpgsqlTestStoreFactory WithConnectionStringOptions(string connectionStringOptions)
        => new(connectionStringOptions);

    protected NpgsqlTestStoreFactory(string connectionStringOptions = null)
    {
        _connectionStringOptions = connectionStringOptions;
    }

    public override TestStore Create(string storeName)
        => NpgsqlTestStore.Create(storeName, _connectionStringOptions);

    public override TestStore GetOrCreate(string storeName)
        => NpgsqlTestStore.GetOrCreate(storeName, connectionStringOptions: _connectionStringOptions);

    public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkNpgsql();
}
