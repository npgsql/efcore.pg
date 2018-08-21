using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities
{
    public class NpgsqlTestStoreFactory : RelationalTestStoreFactory
    {
        public static NpgsqlTestStoreFactory Instance { get; } = new NpgsqlTestStoreFactory();

        protected NpgsqlTestStoreFactory()
        {
        }

        public override TestStore Create(string storeName)
            => NpgsqlTestStore.Create(storeName);

        public override TestStore GetOrCreate(string storeName)
            => NpgsqlTestStore.GetOrCreate(storeName);

        public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkNpgsql();
    }
}
