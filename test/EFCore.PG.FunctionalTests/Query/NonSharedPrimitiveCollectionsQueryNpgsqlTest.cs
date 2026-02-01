using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Query;

public class NonSharedPrimitiveCollectionsQueryNpgsqlTest(NonSharedFixture fixture)
    : NonSharedPrimitiveCollectionsQueryRelationalTestBase(fixture)
{
    protected override DbContextOptionsBuilder SetParameterizedCollectionMode(DbContextOptionsBuilder optionsBuilder, ParameterTranslationMode parameterizedCollectionMode)
    {
        new NpgsqlDbContextOptionsBuilder(optionsBuilder).UseParameterizedCollectionMode(parameterizedCollectionMode);

        return optionsBuilder;
    }

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
            seed: async context =>
            {
                var entry = context.Add(new TestEntity());
                entry.Property<int[,]>("MultidimensionalArray").CurrentValue = new[,] { { 1, 2 }, { 3, 4 } };
                await context.SaveChangesAsync();
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
        await base.Column_collection_inside_json_owned_entity();

        AssertSql(
            """
SELECT t."Id", t."Owned"
FROM "TestOwner" AS t
WHERE jsonb_array_length(t."Owned" -> 'Strings') = 2
LIMIT 2
""",
            //
            """
SELECT t."Id", t."Owned"
FROM "TestOwner" AS t
WHERE (t."Owned" #>> '{Strings,1}') = 'bar'
LIMIT 2
""");
    }

    #region Contains with various index methods

    // For Contains over column collections that have a (modeled) GIN index, we translate to the containment operator (@>).
    // Otherwise we translate to the ANY construct.
    [ConditionalFact]
    public virtual async Task Column_collection_Contains_with_GIN_index_uses_containment()
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onModelCreating: mb => mb.Entity<TestEntity>()
                .HasIndex(e => e.Ints)
                .HasMethod("GIN"),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1, Ints = [1, 2, 3] },
                    new TestEntity { Id = 2, Ints = [1, 2, 4] });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var result = await context.Set<TestEntity>().Where(c => c.Ints!.Contains(4)).SingleAsync();
        Assert.Equal(2, result.Id);

        AssertSql(
            """
SELECT t."Id", t."Ints"
FROM "TestEntity" AS t
WHERE t."Ints" @> ARRAY[4]::integer[]
LIMIT 2
""");
    }

    [ConditionalFact]
    public virtual async Task Column_collection_Contains_with_btree_index_does_not_use_containment()
    {
        var contextFactory = await InitializeAsync<TestContext>(
            onModelCreating: mb => mb.Entity<TestEntity>().HasIndex(e => e.Ints),
            seed: context =>
            {
                context.AddRange(
                    new TestEntity { Id = 1, Ints = [1, 2, 3] },
                    new TestEntity { Id = 2, Ints = [1, 2, 4] });
                return context.SaveChangesAsync();
            });

        await using var context = contextFactory.CreateContext();

        var result = await context.Set<TestEntity>().Where(c => c.Ints!.Contains(4)).SingleAsync();
        Assert.Equal(2, result.Id);

        AssertSql(
            """
SELECT t."Id", t."Ints"
FROM "TestEntity" AS t
WHERE 4 = ANY (t."Ints")
LIMIT 2
""");
    }

    #endregion Contains with various index methods

    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
