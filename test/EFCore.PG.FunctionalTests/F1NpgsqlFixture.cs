using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class F1NpgsqlFixture : F1RelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
        public override TestHelpers TestHelpers => NpgsqlTestHelpers.Instance;

        protected override void BuildModelExternal(ModelBuilder modelBuilder)
        {
            base.BuildModelExternal(modelBuilder);

            modelBuilder.Entity<Chassis>().UseXminAsConcurrencyToken();
            modelBuilder.Entity<Driver>().UseXminAsConcurrencyToken();
            modelBuilder.Entity<Team>().UseXminAsConcurrencyToken();
        }
    }
}
