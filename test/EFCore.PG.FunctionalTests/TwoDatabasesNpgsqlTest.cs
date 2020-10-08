using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class TwoDatabasesNpgsqlTest : TwoDatabasesTestBase, IClassFixture<NpgsqlFixture>
    {
        public TwoDatabasesNpgsqlTest(NpgsqlFixture fixture)
            : base(fixture)
        {
        }

        protected new NpgsqlFixture Fixture => (NpgsqlFixture)base.Fixture;

        protected override DbContextOptionsBuilder CreateTestOptions(
            DbContextOptionsBuilder optionsBuilder, bool withConnectionString = false)
            => withConnectionString
                ? optionsBuilder.UseNpgsql(DummyConnectionString)
                : optionsBuilder.UseNpgsql();

        protected override TwoDatabasesWithDataContext CreateBackingContext(string databaseName)
            => new TwoDatabasesWithDataContext(Fixture.CreateOptions(NpgsqlTestStore.Create(databaseName)));

        protected override string DummyConnectionString { get; } = "Host=localhost;Database=DoesNotExist";
    }
}
