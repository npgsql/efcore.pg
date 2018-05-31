using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities
{
    public class NpgsqlTestStoreFactory : ITestStoreFactory
    {
        public static NpgsqlTestStoreFactory Instance { get; } = new NpgsqlTestStoreFactory();

        protected NpgsqlTestStoreFactory()
        {
        }

        public virtual TestStore Create(string storeName)
            => NpgsqlTestStore.Create(storeName);

        public virtual TestStore GetOrCreate(string storeName)
            => NpgsqlTestStore.GetOrCreate(storeName);

        public IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkNpgsql()
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory());
    }
}
