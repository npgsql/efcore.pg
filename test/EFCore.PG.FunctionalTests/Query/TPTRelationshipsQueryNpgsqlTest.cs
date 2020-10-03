using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class TPTRelationshipsQueryNpgsqlTest
        : TPTRelationshipsQueryTestBase<TPTRelationshipsQueryNpgsqlTest.TPTRelationshipsQueryNpgsqlFixture>
    {
        public TPTRelationshipsQueryNpgsqlTest(
            TPTRelationshipsQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture)
            => fixture.TestSqlLoggerFactory.Clear();

        public class TPTRelationshipsQueryNpgsqlFixture : TPTRelationshipsQueryRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
        }
    }
}
