namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class NpgsqlTestStoreFactory(
        string? scriptPath = null,
        string? additionalSql = null,
        string? connectionStringOptions = null,
        bool useConnectionString = false) : RelationalTestStoreFactory
{
    public static NpgsqlTestStoreFactory Instance { get; } = new();

    public override TestStore Create(string storeName)
        => new NpgsqlTestStore(storeName, scriptPath, additionalSql, connectionStringOptions, shared: false) { UseConnectionString = useConnectionString };

    public override TestStore GetOrCreate(string storeName)
        => new NpgsqlTestStore(storeName, scriptPath, additionalSql, connectionStringOptions, shared: true) { UseConnectionString = useConnectionString };

    public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkNpgsql();
}
