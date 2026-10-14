using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class AdHocMiscellaneousQueryNpgsqlTest : AdHocMiscellaneousQueryRelationalTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    protected override Task Seed2951(Context2951 context)
        => context.Database.ExecuteSqlRawAsync(
            """
CREATE TABLE "ZeroKey" ("Id" int);
INSERT INTO "ZeroKey" VALUES (NULL)
""");

    // Writes DateTime with Kind=Unspecified to timestamptz
    public override Task SelectMany_where_Select(bool async)
        => Task.CompletedTask;

    // Writes DateTime with Kind=Unspecified to timestamptz
    public override Task Subquery_first_member_compared_to_null(bool async)
        => Task.CompletedTask;

    [ConditionalTheory(Skip = "https://github.com/dotnet/efcore/pull/27995/files#r874038747")]
    public override Task StoreType_for_UDF_used(bool async)
        => base.StoreType_for_UDF_used(async);

    [ConditionalFact]
    public virtual async Task Set_operations_over_array_columns_with_different_element_mappings()
    {
        var contextFactory = await InitializeAsync<Context3940>(
            seed: async context =>
            {
                context.Orders.AddRange(
                    new Context3940.Order { Id = 1, Tags = ["a", "x"], Labels = ["b"] },
                    new Context3940.Order { Id = 2, Tags = ["a"], Labels = ["a", "x"] },
                    new Context3940.Order { Id = 3, Tags = ["a"], Labels = ["a"] });
                await context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var union = await context.Orders.Where(o => o.Tags.Union(o.Labels).Contains("x"))
            .OrderBy(o => o.Id).Select(o => o.Id).ToArrayAsync();
        var except = await context.Orders.Where(o => o.Tags.Except(o.Labels).Any()).Select(o => o.Id).ToArrayAsync();

        Assert.Equal([1, 2], union);
        Assert.Equal([1], except);

        AssertSql(
            """
SELECT o."Id"
FROM "Orders" AS o
WHERE 'x' IN (
    SELECT t.value
    FROM unnest(o."Tags") AS t(value)
    UNION
    SELECT l.value
    FROM unnest(o."Labels") AS l(value)
)
ORDER BY o."Id" NULLS FIRST
""",
            //
            """
SELECT o."Id"
FROM "Orders" AS o
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT t.value
        FROM unnest(o."Tags") AS t(value)
        EXCEPT
        SELECT l.value
        FROM unnest(o."Labels") AS l(value)
    ) AS e)
""");
    }

    protected class Context3940(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Order> Orders => Set<Order>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Order>().PrimitiveCollection(o => o.Labels).ElementType().HasMaxLength(10);

        public class Order
        {
            public int Id { get; set; }
            public List<string> Tags { get; set; } = [];
            public List<string> Labels { get; set; } = [];
        }
    }
}
