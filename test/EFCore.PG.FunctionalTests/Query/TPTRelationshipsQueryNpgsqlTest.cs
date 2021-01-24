using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class TPTRelationshipsQueryNpgsqlTest
        : TPTRelationshipsQueryTestBase<TPTRelationshipsQueryNpgsqlTest.TPTRelationshipsQueryNpgsqlFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public TPTRelationshipsQueryNpgsqlTest(
            TPTRelationshipsQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture)
            => fixture.TestSqlLoggerFactory.Clear();

        public class TPTRelationshipsQueryNpgsqlFixture : TPTRelationshipsQueryRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
        }
    }
}
