namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

public class NpgsqlTestStoreFactory : RelationalTestStoreFactory
{
    private readonly Action<NpgsqlDataSourceBuilder> _dataSourceBuilderAction;

    public static NpgsqlTestStoreFactory Instance { get; } = new();

    public static NpgsqlTestStoreFactory WithDataSourceConfiguration(Action<NpgsqlDataSourceBuilder> dataSourceBuilderAction)
        => new(dataSourceBuilderAction);

    protected NpgsqlTestStoreFactory(Action<NpgsqlDataSourceBuilder> dataSourceBuilderAction = null)
        => _dataSourceBuilderAction = dataSourceBuilderAction;

    public override TestStore Create(string storeName)
        => NpgsqlTestStore.Create(storeName, _dataSourceBuilderAction);

    public override TestStore GetOrCreate(string storeName)
        => NpgsqlTestStore.GetOrCreate(storeName, dataSourceBuilderAction: _dataSourceBuilderAction);

    public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkNpgsql();
}
