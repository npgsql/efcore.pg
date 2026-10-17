using System.Collections.Immutable;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class NonSharedPrimitiveCollectionsQueryNpgsqlTest : NonSharedPrimitiveCollectionsQueryRelationalTestBase
{
    #region Support for specific element types

    // Since we just use arrays for primitive collections, there's no need to test each and every element type; arrays are fully typed
    // and don't need any special conversion/handling like in providers which use JSON.

    // Npgsql maps DateTime to timestamp with time zone by default, which requires UTC timestamps.
    public override Task Array_of_DateTime()
        => TestArray(
            new DateTime(2023, 1, 1, 12, 30, 0),
            new DateTime(2023, 1, 2, 12, 30, 0),
            mb => mb.Entity<TestEntity>()
                .Property(typeof(DateTime[]), "SomeArray")
                .HasColumnType("timestamp without time zone[]"));

    // Npgsql maps DateTime to timestamp with time zone by default, which requires UTC timestamps.
    public override Task Array_of_DateTime_with_milliseconds()
        => TestArray(
            new DateTime(2023, 1, 1, 12, 30, 0, 123),
            new DateTime(2023, 1, 1, 12, 30, 0, 124),
            mb => mb.Entity<TestEntity>()
                .Property(typeof(DateTime[]), "SomeArray")
                .HasColumnType("timestamp without time zone[]"));

    // Npgsql maps DateTime to timestamp with time zone by default, which requires UTC timestamps.
    public override Task Array_of_DateTime_with_microseconds()
        => TestArray(
            new DateTime(2023, 1, 1, 12, 30, 0, 123, 456),
            new DateTime(2023, 1, 1, 12, 30, 0, 123, 457),
            mb => mb.Entity<TestEntity>()
                .Property(typeof(DateTime[]), "SomeArray")
                .HasColumnType("timestamp without time zone[]"));

    [ConditionalFact]
    public virtual Task Array_of_DateTime_utc()
        => TestArray(
            new DateTime(2023, 1, 1, 12, 30, 0, DateTimeKind.Utc),
            new DateTime(2023, 1, 2, 12, 30, 0, DateTimeKind.Utc));

    // Npgsql only supports DateTimeOffset with Offset 0 (mapped to timestamp with time zone)
    public override Task Array_of_DateTimeOffset()
        => TestArray(
            new DateTimeOffset(2023, 1, 1, 12, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2023, 1, 2, 12, 30, 0, TimeSpan.Zero));

    [ConditionalFact]
    public override async Task Multidimensional_array_is_not_supported()
    {
        // Multidimensional arrays are supported in PostgreSQL (via the regular array type); the EFCore.PG maps .NET
        // multidimensional arrays. However, arrays of multidimensional arrays aren't supported (since arrays of arrays generally aren't
        // supported).
        var contextFactory = await InitializeAsync<TestContext>(
            mb => mb.Entity<TestEntity>().Property<int[,]>("MultidimensionalArray"),
            seed: context =>
            {
                var entry = context.Add(new TestEntity());
                entry.Property<int[,]>("MultidimensionalArray").CurrentValue = new[,] { { 1, 2 }, { 3, 4 } };
                context.SaveChanges();
            });

        await using var context = contextFactory.CreateContext();

        var arrays = new[] { new[,] { { 1, 2 }, { 3, 4 } }, new[,] { { 1, 2 }, { 3, 5 } } };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                context.Set<TestEntity>().Where(t => arrays.Contains(EF.Property<int[,]>(t, "MultidimensionalArray"))).ToArrayAsync());
    }

    #endregion Support for specific element types

    public override async Task Column_collection_inside_json_owned_entity()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Column_collection_inside_json_owned_entity());

        Assert.Equal(exception.Message, NpgsqlStrings.Ef7JsonMappingNotSupported);
    }

    [ConditionalFact]
    public virtual async Task Set_operations_over_array_columns_with_different_element_mappings()
    {
        var contextFactory = await InitializeAsync<Context3940>(
            seed: context =>
            {
                context.Orders.AddRange(
                    new Context3940.Order { Id = 1, Tags = ["a", "x"], Labels = ["b"] },
                    new Context3940.Order { Id = 2, Tags = ["a"], Labels = ["a", "x"] },
                    new Context3940.Order { Id = 3, Tags = ["a"], Labels = ["a"] });
                context.SaveChanges();
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
    ) AS t0)
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
    public virtual async Task Intersect_any_with_element_converter_and_different_collection_types(
        string propertyName,
        string parameterType)
    {
        var contextFactory = await InitializeAsync<Context3805>(
            seed: context => context.Database.ExecuteSqlRaw(
                """
INSERT INTO "Entities" ("Id", "Array", "List")
VALUES (1, ARRAY[42]::bigint[], ARRAY[42]::bigint[]),
       (2, ARRAY[99]::bigint[], ARRAY[99]::bigint[]),
       (3, ARRAY[]::bigint[], ARRAY[]::bigint[])
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

    [ConditionalFact]
    public virtual async Task Intersect_any_with_element_converter_and_concrete_immutable_list_parameter()
    {
        var contextFactory = await InitializeAsync<Context3805>(
            seed: context => context.Database.ExecuteSqlRaw(
                """
INSERT INTO "Entities" ("Id", "Array", "List")
VALUES (1, ARRAY[42]::bigint[], ARRAY[42]::bigint[]),
       (2, ARRAY[99]::bigint[], ARRAY[99]::bigint[]),
       (3, ARRAY[]::bigint[], ARRAY[]::bigint[])
"""));

        await using var context = contextFactory.CreateContext();
        var values = ImmutableList.Create(new Context3805.WrappedId(42));

        var result = await context.Entities.Where(e => e.List.Intersect(values).Any()).Select(e => e.Id).ToArrayAsync();
        var reversed = await context.Entities.Where(e => values.Intersect(e.List).Any()).Select(e => e.Id).ToArrayAsync();

        Assert.Equal([1], result);
        Assert.Equal([1], reversed);

        AssertSql(
            """
@__values_0={ '42' } (DbType = Object)

SELECT e."Id"
FROM "Entities" AS e
WHERE e."List" && @__values_0
""",
            //
            """
@__values_0={ '42' } (DbType = Object)

SELECT e."Id"
FROM "Entities" AS e
WHERE @__values_0 && e."List"
""");
    }

    protected class Context3805(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Entity> Entities => Set<Entity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity>().PrimitiveCollection(e => e.Array).ElementType().HasConversion<WrappedIdConverter>();
            modelBuilder.Entity<Entity>().PrimitiveCollection(e => e.List).ElementType().HasConversion<WrappedIdConverter>();
        }

        public class Entity
        {
            public int Id { get; set; }
            public WrappedId[] Array { get; set; } = [];
            public List<WrappedId> List { get; set; } = [];
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
            seed: context =>
            {
                context.Entities.AddRange(
                    new Context3805Ranges.Entity { Id = 1, Ranges = [new(1, true, 5, false)], RangeList = [new(1, true, 5, false)] },
                    new Context3805Ranges.Entity { Id = 2, Ranges = [new(10, true, 15, false)], RangeList = [new(10, true, 15, false)] },
                    new Context3805Ranges.Entity { Id = 3, Ranges = [], RangeList = [] });
                context.SaveChanges();
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

    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
