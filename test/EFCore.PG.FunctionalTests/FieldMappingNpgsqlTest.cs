using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class FieldMappingNpgsqlTest
        : FieldMappingTestBase<FieldMappingNpgsqlTest.FieldMappingNpgsqlFixture>
    {
        public FieldMappingNpgsqlTest(FieldMappingNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public class FieldMappingNpgsqlFixture : FieldMappingFixtureBase
        {
            protected override string StoreName { get; } = "FieldMapping";
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
        }
    }
}
