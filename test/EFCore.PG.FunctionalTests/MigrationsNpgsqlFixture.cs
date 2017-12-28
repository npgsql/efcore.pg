using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class MigrationsNpgsqlFixture : MigrationsFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

        public override MigrationsContext CreateContext()
        {
            var options = AddOptions(
                new DbContextOptionsBuilder()
                .UseNpgsql(TestStore.ConnectionString, b => b.ApplyConfiguration().CommandTimeout(NpgsqlTestStore.CommandTimeout)))
                .UseInternalServiceProvider(ServiceProvider)
                .Options;
            return new MigrationsContext(options);
        }
    }
}
