using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class NullSemanticsQueryNpgsqlFixture : NullSemanticsQueryRelationalFixture
    {
        protected override string StoreName { get; } = "NullSemanticsQueryTest";
        protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
    }
}
