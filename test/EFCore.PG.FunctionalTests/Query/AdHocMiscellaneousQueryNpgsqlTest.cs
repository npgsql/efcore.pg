using System.Collections.Immutable;
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
        var contextFactory = await InitializeAsync<Context3805>(
            seed: context => context.Database.ExecuteSqlRawAsync(
                """
INSERT INTO "Entities" ("Id", "Array", "List", "ReadOnly")
VALUES (1, ARRAY[42]::bigint[], ARRAY[42]::bigint[], ARRAY[42]::bigint[]),
       (2, ARRAY[99]::bigint[], ARRAY[99]::bigint[], ARRAY[99]::bigint[]),
       (3, ARRAY[]::bigint[], ARRAY[]::bigint[], ARRAY[]::bigint[])
"""));

        await using var context = contextFactory.CreateContext();

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
@__values_0={ '42' } (DbType = Object)

SELECT e."Id"
FROM "Entities" AS e
WHERE e."{{propertyName}}" && @__values_0
""",
            //
            $$"""
@__values_0={ '42' } (DbType = Object)

SELECT e."Id"
FROM "Entities" AS e
WHERE @__values_0 && e."{{propertyName}}"
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
            public IReadOnlyCollection<WrappedId> ReadOnly { get; set; } = [];
        }

        public readonly record struct WrappedId(long Value);

        private class WrappedIdConverter() : ValueConverter<WrappedId, long>(v => v.Value, v => new WrappedId(v));
    }

    [ConditionalTheory]
    [InlineData(nameof(Context3805Ranges.Entity.Ranges))]
    [InlineData(nameof(Context3805Ranges.Entity.RangeList))]
    public virtual async Task Intersect_any_with_range_array_and_list_parameter(string propertyName)
    {
        var contextFactory = await InitializeAsync<Context3805Ranges>(
            seed: async context =>
            {
                context.Entities.AddRange(
                    new Context3805Ranges.Entity { Id = 1, Ranges = [new(1, true, 5, false)], RangeList = [new(1, true, 5, false)] },
                    new Context3805Ranges.Entity { Id = 2, Ranges = [new(10, true, 15, false)], RangeList = [new(10, true, 15, false)] },
                    new Context3805Ranges.Entity { Id = 3, Ranges = [], RangeList = [] });
                await context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();
        List<NpgsqlRange<int>> ranges = [new(1, true, 5, false)];

        var result = await context.Entities
            .Where(e => EF.Property<IEnumerable<NpgsqlRange<int>>>(e, propertyName).Intersect(ranges).Any())
            .Select(e => e.Id).ToArrayAsync();
        var reversed = await context.Entities
            .Where(e => ranges.Intersect(EF.Property<IEnumerable<NpgsqlRange<int>>>(e, propertyName)).Any())
            .Select(e => e.Id).ToArrayAsync();

        Assert.Equal([1], result);
        Assert.Equal([1], reversed);

        AssertSql(
            $$"""
@__ranges_0={ '[1,5)' } (DbType = Object)

SELECT e."Id"
FROM "Entities" AS e
WHERE e."{{propertyName}}" && @__ranges_0
""",
            //
            $$"""
@__ranges_0={ '[1,5)' } (DbType = Object)

SELECT e."Id"
FROM "Entities" AS e
WHERE @__ranges_0 && e."{{propertyName}}"
""");
    }

    protected class Context3805Ranges(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities => Set<Entity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity>().PrimitiveCollection(e => e.Ranges).HasColumnType("int4range[]");
            modelBuilder.Entity<Entity>().PrimitiveCollection(e => e.RangeList).HasColumnType("int4range[]");
        }

        public class Entity
        {
            public int Id { get; set; }
            public NpgsqlRange<int>[] Ranges { get; set; } = [];
            public IList<NpgsqlRange<int>> RangeList { get; set; } = [];
        }
    }
}
