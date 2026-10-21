using System.Collections;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class BitArrayTranslationsTest : QueryTestBase<BitArrayTranslationsTest.BitArrayQueryFixture>
{
    public BitArrayTranslationsTest(BitArrayQueryFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Get(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => e.Bits.Get(24)).Select(e => e.Id));

        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE get_bit(e."Bits", 24) = 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Get_fixed_length(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => e.FixedBits.Get(24)).Select(e => e.Id));

        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE get_bit(e."FixedBits", 24) = 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Indexer(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => e.Bits[24]).Select(e => e.Id));

        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE get_bit(e."Bits", 24) = 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Get_parameter_index(bool async)
    {
        var index = 24;

        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => e.Bits.Get(index)).Select(e => e.Id));

        AssertSql(
            """
@index='24'

SELECT e."Id"
FROM "Entities" AS e
WHERE get_bit(e."Bits", @index) = 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Get_column_index(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => e.Bits.Get(e.Index)).Select(e => e.Id));

        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE get_bit(e."Bits", e."Index") = 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Get_projection(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().OrderBy(e => e.Id).Select(e => new
            {
                First = e.Bits.Get(0),
                BeforeByteBoundary = e.Bits.Get(7),
                ByteBoundary = e.Bits.Get(8),
                BeforeIntBoundary = e.Bits.Get(31),
                Last = e.Bits.Get(32)
            }),
            assertOrder: true);

        AssertSql(
            """
SELECT get_bit(e."Bits", 0) = 1 AS "First", get_bit(e."Bits", 7) = 1 AS "BeforeByteBoundary", get_bit(e."Bits", 8) = 1 AS "ByteBoundary", get_bit(e."Bits", 31) = 1 AS "BeforeIntBoundary", get_bit(e."Bits", 32) = 1 AS "Last"
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Get_nullable(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => e.NullableBits!.Get(24)).Select(e => e.Id),
            ss => ss.Set<Entity>().Where(e => e.NullableBits != null && e.NullableBits.Get(24)).Select(e => e.Id));

        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE get_bit(e."NullableBits", 24) = 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Get_nullable_projection(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().OrderBy(e => e.Id).Select(e => e.NullableBits == null ? (bool?)null : e.NullableBits.Get(24)),
            assertOrder: true);

        AssertSql(
            """
SELECT CASE
    WHEN e."NullableBits" IS NULL THEN NULL
    ELSE get_bit(e."NullableBits", 24) = 1 AND e."NullableBits" IS NOT NULL
END
FROM "Entities" AS e
ORDER BY e."Id" NULLS FIRST
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Get_negated(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => !e.Bits.Get(24)).Select(e => e.Id));

        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE get_bit(e."Bits", 24) <> 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Get_parameter_instance(bool async)
    {
        var bits = new BitArray([true, false, false, false, false, false, false, false]);

        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => bits.Get(e.Index)).Select(e => e.Id));

        AssertSql(
            """
@bits='System.Collections.BitArray' (DbType = Object)

SELECT e."Id"
FROM "Entities" AS e
WHERE get_bit(@bits, e."Index") = 1
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Get_constant_instance(bool async)
    {
        var bits = new BitArray([true, false, false, false, false, false, false, false]);

        await AssertQuery(
            async,
            ss => ss.Set<Entity>().Where(e => EF.Constant(bits).Get(e.Index)).Select(e => e.Id),
            ss => ss.Set<Entity>().Where(e => bits.Get(e.Index)).Select(e => e.Id));

        AssertSql(
            """
SELECT e."Id"
FROM "Entities" AS e
WHERE get_bit(B'10000000', e."Index") = 1
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class BitArrayQueryContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<Entity> Entities { get; set; }
    }

    public class Entity
    {
        public int Id { get; set; }
        public int Index { get; set; }
        public BitArray Bits { get; set; } = null!;
        public BitArray FixedBits { get; set; } = null!;
        public BitArray? NullableBits { get; set; }
    }

    public class BitArrayQueryFixture : QueryFixtureBase<BitArrayQueryContext>, ITestSqlLoggerFactory
    {
        private readonly BitArrayData _expectedData = new();

        protected override string StoreName
            => "BitArrayQueryTest";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            => modelBuilder.Entity<Entity>().Property(e => e.FixedBits).HasColumnType("bit(33)");

        protected override async Task SeedAsync(BitArrayQueryContext context)
        {
            context.Entities.AddRange(BitArrayData.CreateEntities());
            await context.SaveChangesAsync();
        }

        public override ISetSource GetExpectedData()
            => _expectedData;

        public override IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, object>();

        public override IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, object>();
    }

    private class BitArrayData : ISetSource
    {
        private IReadOnlyList<Entity> Entities { get; } = CreateEntities();

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
            => typeof(TEntity) == typeof(Entity)
                ? (IQueryable<TEntity>)Entities.AsQueryable()
                : throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));

        public static IReadOnlyList<Entity> CreateEntities()
        {
            var bits = new BitArray(33) { [0] = true, [8] = true, [24] = true, [32] = true };
            var otherBits = new BitArray(bits).Not();

            return
            [
                new() { Id = 1, Index = 0, Bits = bits, FixedBits = bits, NullableBits = bits },
                new() { Id = 2, Index = 7, Bits = otherBits, FixedBits = otherBits },
                new() { Id = 3, Index = 1, Bits = otherBits, FixedBits = otherBits, NullableBits = otherBits }
            ];
        }
    }
}
