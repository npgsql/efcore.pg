using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class TPTTableSplittingNpgsqlTest : TPTTableSplittingTestBase
{
    public TPTTableSplittingNpgsqlTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
}