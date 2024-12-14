namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

#nullable enable

public class NpgsqlTestStoreFactory(
        string? scriptPath = null,
        string? additionalSql = null,
        string? connectionStringOptions = null,
        bool useConnectionString = false) : RelationalTestStoreFactory
{
    public static NpgsqlTestStoreFactory Instance { get; } = new();

    public override TestStore Create(string storeName)
        => new NpgsqlTestStore(storeName, scriptPath, additionalSql, connectionStringOptions, shared: false, useConnectionString);

    public override TestStore GetOrCreate(string storeName)
        => new NpgsqlTestStore(storeName, scriptPath, additionalSql, connectionStringOptions, shared: true, useConnectionString);

    public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkNpgsql();
}
