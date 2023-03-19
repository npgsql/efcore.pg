using NetTopologySuite.Geometries;
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

    [ConditionalFact(Skip = "#30630")] // This test will go away
    public override async Task Array_of_geometry_is_not_supported()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => InitializeAsync<TestContext>(
                onConfiguring: options => options.UseNpgsql(o => o.UseNetTopologySuite()),
                addServices: s => s.AddEntityFrameworkNpgsqlNetTopologySuite(),
                onModelCreating: mb => mb.Entity<TestEntity>().Property<Point[]>("Points")));

        Assert.Equal(CoreStrings.PropertyNotMapped("Point[]", "MyEntity", "Points"), exception.Message);
    }

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

        var arrays = new[]
        {
            new[,] { { 1, 2 }, { 3, 4 } },
            new[,] { { 1, 2 }, { 3, 5 } }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            context.Set<TestEntity>().Where(t => arrays.Contains(EF.Property<int[,]>(t, "MultidimensionalArray"))).ToArrayAsync());
    }

    #endregion Support for specific element types

    public override async Task Column_collection_inside_json_owned_entity()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Column_collection_inside_json_owned_entity());

        Assert.Equal(exception.Message, NpgsqlStrings.Ef7JsonMappingNotSupported);
    }

    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;
}
