using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class Ef6GroupByNpgsqlTest : Ef6GroupByTestBase<Ef6GroupByNpgsqlTest.Ef6GroupByNpgsqlFixture>
{
    public Ef6GroupByNpgsqlTest(Ef6GroupByNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Whats_new_2021_sample_3(bool async)
    {
        await base.Whats_new_2021_sample_3(async);

        AssertSql(
            """
SELECT (
    SELECT p1."LastName"
    FROM "Person" AS p1
    WHERE p1."MiddleInitial" = 'Q' AND p1."Age" = 20 AND (p."LastName" = p1."LastName" OR (p."LastName" IS NULL AND p1."LastName" IS NULL))
    LIMIT 1)
FROM "Person" AS p
WHERE p."MiddleInitial" = 'Q' AND p."Age" = 20
GROUP BY p."LastName"
ORDER BY length((
    SELECT p1."LastName"
    FROM "Person" AS p1
    WHERE p1."MiddleInitial" = 'Q' AND p1."Age" = 20 AND (p."LastName" = p1."LastName" OR (p."LastName" IS NULL AND p1."LastName" IS NULL))
    LIMIT 1))::int NULLS FIRST
""");
    }

    public override async Task Whats_new_2021_sample_5(bool async)
    {
        await base.Whats_new_2021_sample_5(async);

        AssertSql(
            """
SELECT (
    SELECT p1."LastName"
    FROM "Person" AS p1
    WHERE p."FirstName" = p1."FirstName" OR (p."FirstName" IS NULL AND p1."FirstName" IS NULL)
    LIMIT 1)
FROM "Person" AS p
GROUP BY p."FirstName"
ORDER BY (
    SELECT p1."LastName"
    FROM "Person" AS p1
    WHERE p."FirstName" = p1."FirstName" OR (p."FirstName" IS NULL AND p1."FirstName" IS NULL)
    LIMIT 1) NULLS FIRST
""");
    }

    public override async Task Whats_new_2021_sample_6(bool async)
    {
        await base.Whats_new_2021_sample_6(async);

        AssertSql(
            """
SELECT (
    SELECT p1."MiddleInitial"
    FROM "Person" AS p1
    WHERE p1."Age" = 20 AND p."Id" = p1."Id"
    LIMIT 1)
FROM "Person" AS p
WHERE p."Age" = 20
GROUP BY p."Id"
ORDER BY (
    SELECT p1."MiddleInitial"
    FROM "Person" AS p1
    WHERE p1."Age" = 20 AND p."Id" = p1."Id"
    LIMIT 1) NULLS FIRST
""");
    }

    [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/issues/27155")]
    public override Task Whats_new_2021_sample_10(bool async)
        => base.Whats_new_2021_sample_10(async);

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class Ef6GroupByNpgsqlFixture : Ef6GroupByFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<OrderForLinq>().Property(o => o.OrderDate).HasColumnType("timestamp");
        }
    }
}
