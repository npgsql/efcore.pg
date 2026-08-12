using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocMiscellaneousQueryNpgsqlTest(NonSharedFixture fixture) : AdHocMiscellaneousQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    protected override DbContextOptionsBuilder SetParameterizedCollectionMode(DbContextOptionsBuilder optionsBuilder, ParameterTranslationMode parameterizedCollectionMode)
    {
        new NpgsqlDbContextOptionsBuilder(optionsBuilder).UseParameterizedCollectionMode(parameterizedCollectionMode);

        return optionsBuilder;
    }

    // Unlike the other providers, EFCore.PG does actually support mapping JsonElement
    public override Task Mapping_JsonElement_property_throws_a_meaningful_exception()
        => Task.CompletedTask;

    protected override Task Seed2951(Context2951 context)
        => context.Database.ExecuteSqlRawAsync(
            """
CREATE TABLE "ZeroKey" ("Id" int);
INSERT INTO "ZeroKey" VALUES (NULL)
""");

    protected override async Task Seed30915(Context30915 context)
    {
        context.Statuses.AddRange(
            new Context30915.PickupStatus30915 { PickupStatusId = 1, Name = "Active" },
            new Context30915.PickupStatus30915 { PickupStatusId = 2, Name = "NoRequests" },
            new Context30915.PickupStatus30915 { PickupStatusId = 3, Name = "Busy" });

        context.Requests.AddRange(
            new Context30915.PickupRequest30915 { PickupStatusId = 1, Priority = 5 },
            new Context30915.PickupRequest30915 { PickupStatusId = 1, Priority = null },
            new Context30915.PickupRequest30915 { PickupStatusId = 3, Priority = 7 });

        await context.SaveChangesAsync();
    }

    // PROVIDER DIVERGENCE: PostgreSQL supports LATERAL, so unlike SQLite (which throws because it
    // has no APPLY) the correlated whole-object DefaultIfEmpty actually translates and materializes
    // correctly here. Override the base (SQLite-shaped) assert-throws with the real-results assertion.
    public override async Task Correlated_SelectMany_DefaultIfEmpty_whole_object()
    {
        var contextFactory = await InitializeNonSharedTest<Context30915>(seed: Seed30915);
        using var context = contextFactory.CreateDbContext();

        var query = from s in context.Statuses
                    from countInfo in context.Requests
                        .Where(r => r.PickupStatusId == s.PickupStatusId)
                        .GroupBy(r => r.PickupStatusId, (k, els) => new { pickupStatusId = k, Count = els.Count() })
                        .DefaultIfEmpty()
                    orderby s.PickupStatusId
                    select new { s.PickupStatusId, countInfo };

        var result = await query.ToListAsync();

        Assert.Equal(3, result.Count);

        // status 1 -> matched, Count 2
        Assert.Equal(1, result[0].PickupStatusId);
        Assert.NotNull(result[0].countInfo);
        Assert.Equal(1, result[0].countInfo.pickupStatusId);
        Assert.Equal(2, result[0].countInfo.Count);

        // status 2 -> no match: whole non-entity object is null
        Assert.Equal(2, result[1].PickupStatusId);
        Assert.Null(result[1].countInfo);

        // status 3 -> matched, Count 1
        Assert.Equal(3, result[2].PickupStatusId);
        Assert.NotNull(result[2].countInfo);
        Assert.Equal(3, result[2].countInfo.pickupStatusId);
        Assert.Equal(1, result[2].countInfo.Count);

        AssertSql(
            """
SELECT s."PickupStatusId", r0."pickupStatusId", r0."Count", r0.marker
FROM "Statuses" AS s
LEFT JOIN LATERAL (
    SELECT r."PickupStatusId" AS "pickupStatusId", count(*)::int AS "Count", 1 AS marker
    FROM "Requests" AS r
    WHERE r."PickupStatusId" = s."PickupStatusId"
    GROUP BY r."PickupStatusId"
) AS r0 ON TRUE
ORDER BY s."PickupStatusId" NULLS FIRST
""");
    }

    // Writes DateTime with Kind=Unspecified to timestamptz
    public override Task SelectMany_where_Select(bool async)
        => Task.CompletedTask;

    // Writes DateTime with Kind=Unspecified to timestamptz
    public override Task Subquery_first_member_compared_to_null(bool async)
        => Task.CompletedTask;

    public override async Task StoreType_for_UDF_used(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context27954Npgsql>();
        using var context = contextFactory.CreateDbContext();

        var date = new DateTime(2012, 12, 12);
        var query1 = context.Set<Context27954Npgsql.MyEntity>().Where(x => x.SomeDate == date);
        var query2 = context.Set<Context27954Npgsql.MyEntity>().Where(x => Context27954Npgsql.MyEntity.Modify(x.SomeDate) == date);

        if (async)
        {
            await query1.ToListAsync();
            await Assert.ThrowsAnyAsync<Exception>(() => query2.ToListAsync());
        }
        else
        {
            query1.ToList();
            Assert.ThrowsAny<Exception>(() => query2.ToList());
        }
    }

    protected class Context27954Npgsql(DbContextOptions options) : DbContext(options)
    {
        public DbSet<MyEntity> MyEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasDbFunction(typeof(MyEntity).GetMethod(nameof(MyEntity.Modify))!)
                .HasName("ModifyDate")
                .HasStoreType("timestamp without time zone");

            modelBuilder.Entity<MyEntity>()
                .Property(e => e.SomeDate)
                .HasColumnType("timestamp without time zone");
        }

        public class MyEntity
        {
            public int Id { get; set; }

            public DateTime SomeDate { get; set; }

            public static DateTime Modify(DateTime date)
                => throw new NotSupportedException();
        }
    }

    [ConditionalFact]
    public virtual async Task Like_with_implicit_escape_does_not_apply_value_converter()
    {
        var contextFactory = await InitializeNonSharedTest<Context3888>(
            seed: async context =>
            {
                context.Entities.Add(new Context3888.Entity { Value = "ABC" });
                await context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

        var pattern = "%ABC%";

        Assert.Equal(1, await context.Entities.CountAsync(e => EF.Functions.Like(e.Value!, pattern)));
        Assert.Equal(1, await context.Entities.CountAsync(e => EF.Functions.ILike(e.Value!, pattern)));

        AssertSql(
            """
@pattern='%ABC%'

SELECT count(*)::int
FROM "Entities" AS e
WHERE e."Value" LIKE @pattern ESCAPE ''
""",
            //
            """
@pattern='%ABC%'

SELECT count(*)::int
FROM "Entities" AS e
WHERE e."Value" ILIKE @pattern ESCAPE ''
""");
    }

    protected class Context3888(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Entity>()
                .Property(e => e.Value)
                .HasConversion(
                    value => string.IsNullOrWhiteSpace(value) ? null : value,
                    value => value ?? string.Empty);

        public class Entity
        {
            public int Id { get; set; }
            public string? Value { get; set; }
        }
    }
}
