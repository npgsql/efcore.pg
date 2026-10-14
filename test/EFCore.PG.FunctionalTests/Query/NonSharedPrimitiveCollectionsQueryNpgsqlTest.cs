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

    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
