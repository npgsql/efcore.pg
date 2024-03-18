using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL;

public class ConcurrencyDetectorEnabledNpgsqlTest : ConcurrencyDetectorEnabledRelationalTestBase<
    ConcurrencyDetectorEnabledNpgsqlTest.ConcurrencyDetectorNpgsqlFixture>
{
    public ConcurrencyDetectorEnabledNpgsqlTest(ConcurrencyDetectorNpgsqlFixture fixture)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
    }

    public override Task FromSql(bool async)
        => ConcurrencyDetectorTest(
            async c => async
                ? await c.Products.FromSqlRaw(@"select * from ""Products""").ToListAsync()
                : c.Products.FromSqlRaw(@"select * from ""Products""").ToList());

    protected override async Task ConcurrencyDetectorTest(Func<ConcurrencyDetectorDbContext, Task<object>> test)
    {
        await base.ConcurrencyDetectorTest(test);

        Assert.Empty(Fixture.TestSqlLoggerFactory.SqlStatements);
    }

    public class ConcurrencyDetectorNpgsqlFixture : ConcurrencyDetectorFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;
    }
}
