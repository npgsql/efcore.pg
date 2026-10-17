using System.Collections.Immutable;
using System.Collections.ObjectModel;
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
        Assert.Equal(1, result[0].PickupStatusId);
        Assert.NotNull(result[0].countInfo);
        Assert.Equal(1, result[0].countInfo.pickupStatusId);
        Assert.Equal(2, result[0].countInfo.Count);
        Assert.Equal(2, result[1].PickupStatusId);
        Assert.Null(result[1].countInfo);
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

    [ConditionalFact]
    public virtual async Task Set_operations_over_array_columns_with_different_element_mappings()
    {
        var contextFactory = await InitializeNonSharedTest<Context3940>(
            seed: async context =>
            {
                context.Orders.AddRange(
                    new Context3940.Order { Id = 1, Tags = ["a", "x"], Labels = ["b"] },
                    new Context3940.Order { Id = 2, Tags = ["a"], Labels = ["a", "x"] },
                    new Context3940.Order { Id = 3, Tags = ["a"], Labels = ["a"] });
                await context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();

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

    [ConditionalTheory]
    [InlineData(nameof(Context3805.Entity.Array), "array")]
    [InlineData(nameof(Context3805.Entity.Array), "list")]
    [InlineData(nameof(Context3805.Entity.Array), "immutable")]
    [InlineData(nameof(Context3805.Entity.Array), "set")]
    [InlineData(nameof(Context3805.Entity.Array), "enumerable")]
    [InlineData(nameof(Context3805.Entity.List), "array")]
    [InlineData(nameof(Context3805.Entity.List), "list")]
    [InlineData(nameof(Context3805.Entity.List), "immutable")]
    [InlineData(nameof(Context3805.Entity.List), "set")]
    [InlineData(nameof(Context3805.Entity.List), "enumerable")]
    [InlineData(nameof(Context3805.Entity.ReadOnly), "array")]
    [InlineData(nameof(Context3805.Entity.ReadOnly), "list")]
    [InlineData(nameof(Context3805.Entity.ReadOnly), "immutable")]
    [InlineData(nameof(Context3805.Entity.ReadOnly), "set")]
    [InlineData(nameof(Context3805.Entity.ReadOnly), "enumerable")]
    public virtual async Task Intersect_any_with_element_converter_and_different_collection_types(
        string propertyName,
        string parameterType)
    {
        var contextFactory = await InitializeNonSharedTest<Context3805>(
            seed: context => context.Database.ExecuteSqlRawAsync(
                """
INSERT INTO "Entities" ("Id", "Array", "List", "ReadOnly")
VALUES (1, ARRAY[42]::bigint[], ARRAY[42]::bigint[], ARRAY[42]::bigint[]),
       (2, ARRAY[99]::bigint[], ARRAY[99]::bigint[], ARRAY[99]::bigint[]),
       (3, ARRAY[]::bigint[], ARRAY[]::bigint[], ARRAY[]::bigint[])
"""));

        await using var context = contextFactory.CreateDbContext();

        IEnumerable<Context3805.WrappedId> values = parameterType switch
        {
            "array" => new Context3805.WrappedId[] { new(42) },
            "list" => new List<Context3805.WrappedId> { new(42) },
            "immutable" => ImmutableList.Create(new Context3805.WrappedId(42)),
            "set" => new HashSet<Context3805.WrappedId> { new(42) },
            "enumerable" => new[] { new Context3805.WrappedId(42) }.Where(_ => true),
            _ => throw new ArgumentOutOfRangeException(nameof(parameterType))
        };

        var result = await context.Entities
            .Where(e => EF.Property<IEnumerable<Context3805.WrappedId>>(e, propertyName).Intersect(values).Any())
            .Select(e => e.Id).ToArrayAsync();
        var reversed = await context.Entities
            .Where(e => values.Intersect(EF.Property<IEnumerable<Context3805.WrappedId>>(e, propertyName)).Any())
            .Select(e => e.Id).ToArrayAsync();

        Assert.Equal([1], result);
        Assert.Equal([1], reversed);

        AssertSql(
            $$"""
@values={ '42' } (DbType = Object)

SELECT e."Id"
FROM "Entities" AS e
WHERE e."{{propertyName}}" && @values
""",
            //
            $$"""
@values={ '42' } (DbType = Object)

SELECT e."Id"
FROM "Entities" AS e
WHERE @values && e."{{propertyName}}"
""");
    }

    protected class Context3805(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities => Set<Entity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity>().PrimitiveCollection(e => e.Array).ElementType().HasConversion<WrappedIdConverter>();
            modelBuilder.Entity<Entity>().PrimitiveCollection(e => e.List).ElementType().HasConversion<WrappedIdConverter>();
            modelBuilder.Entity<Entity>().PrimitiveCollection(e => e.ReadOnly).ElementType().HasConversion<WrappedIdConverter>();
        }

        public class Entity
        {
            public int Id { get; set; }
            public WrappedId[] Array { get; set; } = [];
            public List<WrappedId> List { get; set; } = [];
            public ReadOnlyCollection<WrappedId> ReadOnly { get; set; } = new([]);
        }

        public readonly record struct WrappedId(long Value);

        private class WrappedIdConverter() : ValueConverter<WrappedId, long>(v => v.Value, v => new WrappedId(v));
    }

    [ConditionalFact]
    public virtual async Task Intersect_any_with_range_array_and_list_parameter()
    {
        var contextFactory = await InitializeNonSharedTest<Context3805Ranges>(
            seed: async context =>
            {
                context.Entities.AddRange(
                    new Context3805Ranges.Entity { Id = 1, Ranges = [new(1, true, 5, false)] },
                    new Context3805Ranges.Entity { Id = 2, Ranges = [new(10, true, 15, false)] },
                    new Context3805Ranges.Entity { Id = 3, Ranges = [] });
                await context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateDbContext();
        List<NpgsqlRange<int>> ranges = [new(1, true, 5, false)];

        var result = await context.Entities.Where(e => e.Ranges.Intersect(ranges).Any()).Select(e => e.Id).ToArrayAsync();
        var reversed = await context.Entities.Where(e => ranges.Intersect(e.Ranges).Any()).Select(e => e.Id).ToArrayAsync();

        Assert.Equal([1], result);
        Assert.Equal([1], reversed);

        AssertSql(
            """
@ranges={ '[1,5)' } (DbType = Object)

SELECT e."Id"
FROM "Entities" AS e
WHERE e."Ranges" && @ranges
""",
            //
            """
@ranges={ '[1,5)' } (DbType = Object)

SELECT e."Id"
FROM "Entities" AS e
WHERE @ranges && e."Ranges"
""");
    }

    protected class Context3805Ranges(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities => Set<Entity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Entity>().PrimitiveCollection(e => e.Ranges).HasColumnType("int4range[]");

        public class Entity
        {
            public int Id { get; set; }
            public NpgsqlRange<int>[] Ranges { get; set; } = [];
        }
    }
}
