#pragma warning disable EF8001 // ToJson on owned entities is obsolete

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class AdHocJsonQueryNpgsqlTest(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    protected override string NonSharedStoreName
        => "AdHocJsonQueryTests";

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_predicate_on_bytea(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<TypesDbContext>(
            seed: async context =>
            {
                context.Entities.AddRange(
                    new TypesContainerEntity { JsonEntity = new TypesJsonEntity { Bytea = [1, 2, 3] } },
                    new TypesContainerEntity { JsonEntity = new TypesJsonEntity { Bytea = [1, 2, 4] } });
                await context.SaveChangesAsync();
            });

        using (var context = contextFactory.CreateDbContext())
        {
            var query = context.Entities.Where(x => x.JsonEntity.Bytea == new byte[] { 1, 2, 4 });

            var result = async
                ? await query.SingleAsync()
                : query.Single();

            Assert.Equal(2, result.Id);

            AssertSql(
                """
SELECT e."Id", e."JsonEntity"
FROM "Entities" AS e
WHERE (decode(e."JsonEntity" ->> 'Bytea', 'base64')) = BYTEA E'\\x010204'
LIMIT 2
""");
        }
    }

    [ConditionalTheory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Json_predicate_on_interval(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<TypesDbContext>(
            seed: async context =>
            {
                context.Entities.AddRange(
                    new TypesContainerEntity { JsonEntity = new TypesJsonEntity { Interval = new TimeSpan(1, 2, 3, 4, 123, 456) } },
                    new TypesContainerEntity { JsonEntity = new TypesJsonEntity { Interval = new TimeSpan(2, 2, 3, 4, 123, 456) } });
                await context.SaveChangesAsync();
            });

        using (var context = contextFactory.CreateDbContext())
        {
            var query = context.Entities.Where(x => x.JsonEntity.Interval == new TimeSpan(2, 2, 3, 4, 123, 456));

            var result = async
                ? await query.SingleAsync()
                : query.Single();

            Assert.Equal(2, result.Id);

            AssertSql(
                """
SELECT e."Id", e."JsonEntity"
FROM "Entities" AS e
WHERE (CAST(e."JsonEntity" ->> 'Interval' AS interval)) = INTERVAL '2 02:03:04.123456'
LIMIT 2
""");
        }
    }

    protected class TypesDbContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<TypesContainerEntity> Entities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<TypesContainerEntity>().OwnsOne(b => b.JsonEntity).ToJson();
    }

    public class TypesContainerEntity
    {
        public int Id { get; set; }
        public TypesJsonEntity JsonEntity { get; set; }
    }

    public class TypesJsonEntity
    {
        public byte[] Bytea { get; set; }
        public TimeSpan Interval { get; set; }
    }

    protected void AssertSql(params string[] expected)
        => ((TestSqlLoggerFactory)ListLoggerFactory).AssertBaseline(expected);
}
