using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class F1UIntNpgsqlFixture : F1NpgsqlFixtureBase<uint?>
    {
    }

    public class F1NpgsqlFixture : F1NpgsqlFixtureBase<byte[]>
    {
    }

    // Note that the type parameter determines the type of the Version shadow property in the model,
    // but in Npgsql we just use UseXminAsConcurrencyToken.
    public abstract class F1NpgsqlFixtureBase<TRowVersion> : F1RelationalFixture<TRowVersion>
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
