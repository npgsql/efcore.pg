using System.Collections.Immutable;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
// ReSharper disable ConvertToConstant.Local

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class DictionaryEntity
{
    public int Id { get; set; }

    public Dictionary<string, string> Dictionary { get; set; } = null!;

    public ImmutableDictionary<string, string> ImmutableDictionary { get; set; } = null!;
    public Dictionary<string, string> JsonDictionary { get; set; } = null!;
    public ImmutableDictionary<string, string> JsonbDictionary { get; set; } = null!;
    public Dictionary<string, int> IntDictionary { get; set; } = null!;
    public Dictionary<string, Dictionary<string, string>> NestedDictionary { get; set; }

}

public class DictionaryQueryContext(DbContextOptions options) : PoolableDbContext(options)
{
    public DbSet<DictionaryEntity> SomeEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<DictionaryEntity>();
        entity.Property(_ => _.JsonDictionary).HasColumnType("json").IsRequired();
        entity.Property(_ => _.JsonbDictionary).HasColumnType("jsonb").IsRequired();
        entity.Property(_ => _.IntDictionary).HasColumnType("jsonb").IsRequired();
        entity.Property(_ => _.NestedDictionary).HasColumnType("jsonb").IsRequired();
    }

    public static async Task SeedAsync(DictionaryQueryContext context)
    {
        var arrayEntities = DictionaryQueryData.CreateDictionaryEntities();

        context.SomeEntities.AddRange(arrayEntities);
        await context.SaveChangesAsync();
    }
}

public class DictionaryQueryData : ISetSource
{
    public IReadOnlyList<DictionaryEntity> DictionaryEntities { get; } = CreateDictionaryEntities();

    public IQueryable<TEntity> Set<TEntity>()
        where TEntity : class
    {
        if (typeof(TEntity) == typeof(DictionaryEntity))
        {
            return (IQueryable<TEntity>)DictionaryEntities.AsQueryable();
        }

        throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
    }

    public static IReadOnlyList<DictionaryEntity> CreateDictionaryEntities()
        =>
        [
            new()
            {
                Id = 1,
                Dictionary = new() { ["key"] = "value" },
                ImmutableDictionary = new Dictionary<string, string> { ["key2"] = "value2" }.ToImmutableDictionary(),
                JsonDictionary = new() { ["jkey"] = "value" },
                JsonbDictionary = new Dictionary<string, string> { ["jkey2"] = "value" }.ToImmutableDictionary(),
                IntDictionary = new() { ["ikey"] = 1},
                NestedDictionary = new() { ["key"] = new() { ["nested"] = "value"}}
            },
            new()
            {
                Id = 2,
                Dictionary = new() { ["key"] = "value" },
                ImmutableDictionary = new Dictionary<string, string> { ["key3"] = "value3" }.ToImmutableDictionary(),
                JsonDictionary = new() { ["jkey"] = "value" },
                JsonbDictionary = new Dictionary<string, string> { ["jkey2"] = "value2" }.ToImmutableDictionary(),
                IntDictionary = new() { ["ikey"] = 2},
                NestedDictionary = new() { ["key"] = new() { ["nested2"] = "value2"}}
            }
        ];
}

public class DictionaryQueryFixture : SharedStoreFixtureBase<DictionaryQueryContext>, IQueryFixtureBase, ITestSqlLoggerFactory
{
    static DictionaryQueryFixture()
    {
        // TODO: Switch to using NpgsqlDataSource
#pragma warning disable CS0618 // Type or member is obsolete
        NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
#pragma warning restore CS0618 // Type or member is obsolete
    }

    protected override string StoreName
        => "HstoreQueryTest";

    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    private DictionaryQueryData _expectedData;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(wcb => wcb.Ignore(CoreEventId.CollectionWithoutComparer));

    protected override Task SeedAsync(DictionaryQueryContext context)
        => DictionaryQueryContext.SeedAsync(context);

    public Func<DbContext> GetContextCreator()
        => CreateContext;

    public ISetSource GetExpectedData()
        => _expectedData ??= new DictionaryQueryData();

    public IReadOnlyDictionary<Type, object> EntitySorters
        => new Dictionary<Type, Func<object, object>>
        {
            { typeof(DictionaryEntity), e => ((DictionaryEntity)e)?.Id }
        }.ToDictionary(e => e.Key, e => (object)e.Value);

    public IReadOnlyDictionary<Type, object> EntityAsserters
        => new Dictionary<Type, Action<object, object>>
        {
            {
                typeof(DictionaryEntity), (e, a) =>
                {
                    Assert.Equal(e is null, a is null);
                    if (a is not null)
                    {
                        var ee = (DictionaryEntity)e;
                        var aa = (DictionaryEntity)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.Dictionary, ee.Dictionary);
                        Assert.Equal(ee.ImmutableDictionary, ee.ImmutableDictionary);
                        Assert.Equal(ee.JsonDictionary, ee.JsonDictionary);
                        Assert.Equal(ee.JsonbDictionary, ee.JsonbDictionary);
                        Assert.Equal(ee.IntDictionary, ee.IntDictionary);
                        Assert.Equal(ee.NestedDictionary, ee.NestedDictionary);
                    }
                }
            }
        }.ToDictionary(e => e.Key, e => (object)e.Value);
}

public class DictionaryQueryTest : QueryTestBase<DictionaryQueryFixture>
{
    public DictionaryQueryTest(DictionaryQueryFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Dictionary_ContainsKey(bool async)
    {
        var keyToTest = "key";
        await AssertQuery(async, ss => ss.Set<DictionaryEntity>().Where(s => s.Dictionary.ContainsKey(keyToTest)));
        AssertSql("""
@__keyToTest_0='key'

SELECT s."Id", s."Dictionary", s."ImmutableDictionary", s."IntDictionary", s."JsonDictionary", s."JsonbDictionary", s."NestedDictionary"
FROM "SomeEntities" AS s
WHERE s."Dictionary" ? @__keyToTest_0
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task JsonbDictionary_ContainsKey(bool async)
    {
        var keyToTest = "jkey2";
        await AssertQuery(async, ss => ss.Set<DictionaryEntity>().Where(s => s.JsonbDictionary.ContainsKey(keyToTest)));
        AssertSql("""
@__keyToTest_0='jkey2'

SELECT s."Id", s."Dictionary", s."ImmutableDictionary", s."IntDictionary", s."JsonDictionary", s."JsonbDictionary", s."NestedDictionary"
FROM "SomeEntities" AS s
WHERE s."JsonbDictionary" ? @__keyToTest_0
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ImmutableDictionary_ContainsKey(bool async)
    {
        var keyToTest = "key3";
        await AssertQuery(async, ss => ss.Set<DictionaryEntity>().Where(s => s.ImmutableDictionary.ContainsKey(keyToTest)));
        AssertSql(
            """
@__keyToTest_0='key3'

SELECT s."Id", s."Dictionary", s."ImmutableDictionary", s."IntDictionary", s."JsonDictionary", s."JsonbDictionary", s."NestedDictionary"
FROM "SomeEntities" AS s
WHERE s."ImmutableDictionary" ? @__keyToTest_0
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Dictionary_ContainsValue(bool async)
    {
        var valueToTest = "value";
        await AssertQuery(async, ss => ss.Set<DictionaryEntity>().Where(s => s.Dictionary.ContainsValue(valueToTest)));
        AssertSql(
            """
@__valueToTest_0='value'

SELECT s."Id", s."Dictionary", s."ImmutableDictionary", s."IntDictionary", s."JsonDictionary", s."JsonbDictionary", s."NestedDictionary"
FROM "SomeEntities" AS s
WHERE @__valueToTest_0 = ANY (avals(s."Dictionary"))
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ImmutableDictionary_ContainsValue(bool async)
    {
        var valueToTest = "value2";
        await AssertQuery(async, ss => ss.Set<DictionaryEntity>().Where(s => s.ImmutableDictionary.ContainsValue(valueToTest)));
        AssertSql(
            """
@__valueToTest_0='value2'

SELECT s."Id", s."Dictionary", s."ImmutableDictionary", s."IntDictionary", s."JsonDictionary", s."JsonbDictionary", s."NestedDictionary"
FROM "SomeEntities" AS s
WHERE @__valueToTest_0 = ANY (avals(s."ImmutableDictionary"))
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Dictionary_Keys_ToList(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.Dictionary.Keys.ToList()), elementAsserter: Assert.Equal,
            assertOrder: true);
        AssertSql(
            """
SELECT akeys(s."Dictionary")
FROM "SomeEntities" AS s
""");
    }

    // Note: There is no "Dictionary_Keys" or "Dictionary_Values" tests as they return a Dictionary<string,string>.KeyCollection and Dictionary<string,string>.ValueCollection
    // which cannot be translated from a `List<string>` which is what the `avals` and `akeys` functions returns. ImmutableDictionary<string,string>.Keys and ImmutableDictionary<string,string>.Values
    // does have tests as they return an `IEnumerable<string>` that `List<string>` is compatible with
    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ImmutableDictionary_Keys(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.ImmutableDictionary.Keys), elementAsserter: Assert.Equal,
            assertOrder: true);
        AssertSql(
            """
SELECT akeys(s."ImmutableDictionary")
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task JsonbDictionary_Keys(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.JsonbDictionary.Keys), elementAsserter: Assert.Equal,
            assertOrder: true);
        AssertSql(
            """
SELECT array((
    SELECT jsonb_object_keys(s."JsonbDictionary")))
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task JsonDictionary_Keys_ToList(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.JsonDictionary.Keys.ToList()), elementAsserter: Assert.Equal,
            assertOrder: true);
        AssertSql(
            """
SELECT array((
    SELECT json_object_keys(s."JsonDictionary")))
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task JsonbDictionary_Keys_ToList(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.JsonbDictionary.Keys.ToList()), elementAsserter: Assert.Equal,
            assertOrder: true);
        AssertSql(
            """
SELECT array((
    SELECT jsonb_object_keys(s."JsonbDictionary")))
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ImmutableDictionary_Keys_ToList(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.ImmutableDictionary.Keys.ToList()), elementAsserter: Assert.Equal,
            assertOrder: true);
        AssertSql(
            """
SELECT akeys(s."ImmutableDictionary")
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Dictionary_Values_ToList(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.Dictionary.Values.ToList()), elementAsserter: Assert.Equal,
            assertOrder: true);
        AssertSql(
            """
SELECT avals(s."Dictionary")
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ImmutableDictionary_Values(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.ImmutableDictionary.Values), elementAsserter: Assert.Equal,
            assertOrder: true);
        AssertSql(
            """
SELECT avals(s."ImmutableDictionary")
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ImmutableDictionary_Values_ToList(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.ImmutableDictionary.Values.ToList()), elementAsserter: Assert.Equal,
            assertOrder: true);
        AssertSql(
            """
SELECT avals(s."ImmutableDictionary")
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task JsonDictionary_Values_ToList(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.JsonDictionary.Values.ToList()), elementAsserter: Assert.Equal,
            assertOrder: true);
        AssertSql(
            """
SELECT (
    SELECT array_agg(j.value)
    FROM json_each_text(s."JsonDictionary") AS j)
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task JsonbDictionary_Values_ToList(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.JsonbDictionary.Values.ToList()), elementAsserter: Assert.Equal,
            assertOrder: true);
        AssertSql(
            """
SELECT (
    SELECT array_agg(j.value)
    FROM jsonb_each_text(s."JsonbDictionary") AS j)
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task IntDictionary_Values_ToList(bool async)
    {
        await AssertQuery(
            async, ss =>
                ss.Set<DictionaryEntity>().Select(s => s.IntDictionary.Values.ToList()), elementAsserter: Assert.Equal,
            assertOrder: true);
        AssertSql(
            """
SELECT (
    SELECT array_agg(j.value::int)
    FROM jsonb_each(s."IntDictionary") AS j)
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task NestedDictionary_Values_ToList(bool async)
    {
        await AssertQuery(
            async, ss =>
                ss.Set<DictionaryEntity>().Select(s => s.NestedDictionary.Values.ToList()), elementAsserter: Assert.Equal,
            assertOrder: true);
        AssertSql(
            """
SELECT (
    SELECT array_agg(j.value)
    FROM jsonb_each(s."NestedDictionary") AS j)
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Dictionary_Item_equals(bool async)
    {
        var keyToTest = "key";
        var valueToTest = "value";
        await AssertQuery(async, ss => ss.Set<DictionaryEntity>().Where(s => s.Dictionary[keyToTest] == valueToTest));
        AssertSql(
            """
@__valueToTest_0='value'

SELECT s."Id", s."Dictionary", s."ImmutableDictionary", s."IntDictionary", s."JsonDictionary", s."JsonbDictionary", s."NestedDictionary"
FROM "SomeEntities" AS s
WHERE s."Dictionary" -> 'key' = @__valueToTest_0
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ImmutableDictionary_Item_equals(bool async)
    {
        var keyToTest = "key2";
        var valueToTest = "value2";
        await AssertQuery(async, ss =>
                ss.Set<DictionaryEntity>().Where(s => s.ImmutableDictionary[keyToTest] == valueToTest),
            ss => ss.Set<DictionaryEntity>().Where(s =>
                s.ImmutableDictionary.ContainsKey(keyToTest) && s.ImmutableDictionary[keyToTest] == valueToTest));
        AssertSql(
            """
@__keyToTest_0='key2'
@__valueToTest_1='value2'

SELECT s."Id", s."Dictionary", s."ImmutableDictionary", s."IntDictionary", s."JsonDictionary", s."JsonbDictionary", s."NestedDictionary"
FROM "SomeEntities" AS s
WHERE s."ImmutableDictionary" -> @__keyToTest_0 = @__valueToTest_1
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task JsonDictionary_Item_equals(bool async)
    {
        var keyToTest = "jkey";
        var valueToTest = "value";
        await AssertQuery(async, ss =>
            ss.Set<DictionaryEntity>().Where(s => s.JsonDictionary[keyToTest] == valueToTest));
        AssertSql(
            """
@__valueToTest_0='value'

SELECT s."Id", s."Dictionary", s."ImmutableDictionary", s."IntDictionary", s."JsonDictionary", s."JsonbDictionary", s."NestedDictionary"
FROM "SomeEntities" AS s
WHERE s."JsonDictionary" ->> 'jkey' = @__valueToTest_0
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task JsonbDictionary_Item_equals(bool async)
    {
        var keyToTest = "jkey2";
        var valueToTest = "value2";
        await AssertQuery(async, ss =>
            ss.Set<DictionaryEntity>().Where(s => s.JsonbDictionary[keyToTest] == valueToTest));
        AssertSql(
            """
@__keyToTest_0='jkey2'
@__valueToTest_1='value2'

SELECT s."Id", s."Dictionary", s."ImmutableDictionary", s."IntDictionary", s."JsonDictionary", s."JsonbDictionary", s."NestedDictionary"
FROM "SomeEntities" AS s
WHERE s."JsonbDictionary" ->> @__keyToTest_0 = @__valueToTest_1
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task IntDictionary_Item_equals(bool async)
    {
        var keyToTest = "ikey";
        var valueToTest = 1;
        await AssertQuery(async, ss =>
            ss.Set<DictionaryEntity>().Where(s => s.IntDictionary[keyToTest] == valueToTest));
        AssertSql(
            """
@__valueToTest_0='1' (DbType = Object)

SELECT s."Id", s."Dictionary", s."ImmutableDictionary", s."IntDictionary", s."JsonDictionary", s."JsonbDictionary", s."NestedDictionary"
FROM "SomeEntities" AS s
WHERE s."IntDictionary" -> 'ikey' = @__valueToTest_0
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task NestedDictionary_Item_equals(bool async)
    {
        var keyToTest = "key";
        var key2ToTest = "nested";
        var valueToTest = "value";
        await AssertQuery(
            async, ss =>
                ss.Set<DictionaryEntity>().Where(s => s.NestedDictionary[keyToTest][key2ToTest] == valueToTest),
            ss => ss.Set<DictionaryEntity>().Where(
                s => s.NestedDictionary.ContainsKey(keyToTest)
                    && s.NestedDictionary[keyToTest].ContainsKey(key2ToTest)
                    && s.NestedDictionary[keyToTest][key2ToTest] == valueToTest));
        AssertSql(
            """
@__valueToTest_0='value'

SELECT s."Id", s."Dictionary", s."ImmutableDictionary", s."IntDictionary", s."JsonDictionary", s."JsonbDictionary", s."NestedDictionary"
FROM "SomeEntities" AS s
WHERE s."NestedDictionary" -> 'key' ->> 'nested' = @__valueToTest_0
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Dictionary_Where_Count(bool async)
    {
        await AssertQuery(async, ss => ss.Set<DictionaryEntity>().Where(s => s.Dictionary.Count >= 1));
        AssertSql(
            """
SELECT s."Id", s."Dictionary", s."ImmutableDictionary", s."IntDictionary", s."JsonDictionary", s."JsonbDictionary", s."NestedDictionary"
FROM "SomeEntities" AS s
WHERE cardinality(akeys(s."Dictionary")) >= 1
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Dictionary_Select_Count(bool async)
    {
        await AssertQuery(async, ss => ss.Set<DictionaryEntity>().Select(s => s.Dictionary.Count));
        AssertSql(
            """
SELECT cardinality(akeys(s."Dictionary"))
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ImmutableDictionary_Where_Count(bool async)
    {
        await AssertQuery(async, ss => ss.Set<DictionaryEntity>().Where(s => s.ImmutableDictionary.Count >= 1));
        AssertSql(
            """
SELECT s."Id", s."Dictionary", s."ImmutableDictionary", s."IntDictionary", s."JsonDictionary", s."JsonbDictionary", s."NestedDictionary"
FROM "SomeEntities" AS s
WHERE cardinality(akeys(s."ImmutableDictionary")) >= 1
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ImmutableDictionary_Select_Count(bool async)
    {
        await AssertQuery(async, ss => ss.Set<DictionaryEntity>().Select(s => s.ImmutableDictionary.Count));
        AssertSql(
            """
SELECT cardinality(akeys(s."ImmutableDictionary"))
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Enumerable_KeyValuePair_Count(bool async)
    {
        await AssertQuery(
            // ReSharper disable once UseCollectionCountProperty
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.Dictionary.Count()));
        AssertSql(
            """
SELECT cardinality(akeys(s."Dictionary"))
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ImmutableDictionary_Where_IsEmpty(bool async)
    {
        await AssertQuery(async, ss => ss.Set<DictionaryEntity>().Where(s => !s.ImmutableDictionary.IsEmpty));
        AssertSql(
            """
SELECT s."Id", s."Dictionary", s."ImmutableDictionary", s."IntDictionary", s."JsonDictionary", s."JsonbDictionary", s."NestedDictionary"
FROM "SomeEntities" AS s
WHERE cardinality(akeys(s."ImmutableDictionary")) <> 0
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task JsonbDictionary_Where_IsEmpty(bool async)
    {
        await AssertQuery(async, ss => ss.Set<DictionaryEntity>().Where(s => !s.JsonbDictionary.IsEmpty));
        AssertSql(
            """
SELECT s."Id", s."Dictionary", s."ImmutableDictionary", s."IntDictionary", s."JsonDictionary", s."JsonbDictionary", s."NestedDictionary"
FROM "SomeEntities" AS s
WHERE s."JsonbDictionary" <> '{}'
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ImmutableDictionary_Remove(bool async)
    {
        var key = "key";
        await AssertQuery(async, ss => ss.Set<DictionaryEntity>().Select(s => s.ImmutableDictionary.Remove(key)),
            elementAsserter: AssertEqualsIgnoringOrder, assertOrder: true);
        AssertSql(
            """
@__key_0='key'

SELECT s."ImmutableDictionary" - @__key_0
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Dictionary_Where_Any(bool async)
    {
        await AssertQuery(async, ss => ss.Set<DictionaryEntity>().Where(s => s.Dictionary.Any()));
        AssertSql(
            """
SELECT s."Id", s."Dictionary", s."ImmutableDictionary", s."IntDictionary", s."JsonDictionary", s."JsonbDictionary", s."NestedDictionary"
FROM "SomeEntities" AS s
WHERE cardinality(akeys(s."Dictionary")) <> 0
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ImmutableDictionary_Where_Any(bool async)
    {
        await AssertQuery(async, ss => ss.Set<DictionaryEntity>().Where(s => s.ImmutableDictionary.Any()));
        AssertSql(
            """
SELECT s."Id", s."Dictionary", s."ImmutableDictionary", s."IntDictionary", s."JsonDictionary", s."JsonbDictionary", s."NestedDictionary"
FROM "SomeEntities" AS s
WHERE cardinality(akeys(s."ImmutableDictionary")) <> 0
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Dictionary_ToDictionary(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.Dictionary.ToDictionary()),
            elementAsserter: Assert.Equal, assertOrder: true);
        AssertSql(
            """
SELECT s."Dictionary"
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task JsonDictionary_ToDictionary(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.JsonDictionary.ToDictionary()),
            elementAsserter: Assert.Equal, assertOrder: true);
        AssertSql(
            """
SELECT s."JsonDictionary"
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task JsonDictionary_ToImmutableDictionary(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.JsonDictionary.ToImmutableDictionary()),
            elementAsserter: Assert.Equal, assertOrder: true);
        AssertSql(
            """
SELECT s."JsonDictionary"
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task JsonbDictionary_ToDictionary(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.JsonbDictionary.ToDictionary()),
            elementAsserter: Assert.Equal, assertOrder: true);
        AssertSql(
            """
SELECT s."JsonbDictionary"
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task JsonbDictionary_ToImmutableDictionary(bool async)
    {
        await AssertQuery(
#pragma warning disable CA2009
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.JsonbDictionary.ToImmutableDictionary()),
#pragma warning restore CA2009
            elementAsserter: Assert.Equal, assertOrder: true);
        AssertSql(
            """
SELECT s."JsonbDictionary"
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ImmutableDictionary_ToImmutableDictionary(bool async)
    {
        await AssertQuery(
#pragma warning disable CA2009
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.ImmutableDictionary.ToImmutableDictionary()),
#pragma warning restore CA2009
            elementAsserter: Assert.Equal, assertOrder: true);
        AssertSql(
            """
SELECT s."ImmutableDictionary"
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Dictionary_ToImmutableDictionary(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.Dictionary.ToImmutableDictionary()),
            elementAsserter: Assert.Equal, assertOrder: true);
        AssertSql(
            """
SELECT s."Dictionary"::hstore
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ImmutableDictionary_ToDictionary(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.ImmutableDictionary.ToDictionary()),
            elementAsserter: Assert.Equal, assertOrder: true);
        AssertSql(
            """
SELECT s."ImmutableDictionary"::hstore
FROM "SomeEntities" AS s
""");

    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ImmutableDictionary_Concat_Dictionary(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.ImmutableDictionary.Concat(s.Dictionary)),
            elementAsserter: AssertEqualsIgnoringOrder, assertOrder: true);
        AssertSql(
            """
SELECT s."ImmutableDictionary" || s."Dictionary"
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Dictionary_Concat_ImmutableDictionary(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.Dictionary.Concat(s.ImmutableDictionary)),
            elementAsserter: AssertEqualsIgnoringOrder, assertOrder: true);
        AssertSql(
            """
SELECT s."Dictionary" || s."ImmutableDictionary"
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task JsonbDictionary_Concat_ImmutableDictionary(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.JsonbDictionary.Concat(s.ImmutableDictionary)),
            elementAsserter: AssertEqualsIgnoringOrder, assertOrder: true);
        AssertSql(
            """
SELECT (
    SELECT hstore(array_agg(j.key), array_agg(j.value))
    FROM jsonb_each_text(s."JsonbDictionary") AS j) || s."ImmutableDictionary"
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task JsonbDictionary_Concat_JsonbDictionary(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.JsonbDictionary.Concat(s.JsonbDictionary)),
            elementAsserter: AssertEqualsIgnoringOrder, assertOrder: true);
        AssertSql(
            """
SELECT s."JsonbDictionary" || s."JsonbDictionary"
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task JsonDictionary_Concat_JsonbDictionary(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.JsonDictionary.Concat(s.JsonbDictionary)),
            elementAsserter: AssertEqualsIgnoringOrder, assertOrder: true);
        AssertSql(
            """
SELECT s."JsonDictionary"::jsonb || s."JsonbDictionary"
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task JsonDictionary_Concat_JsonDictionary(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.JsonDictionary.Concat(s.JsonDictionary)),
            elementAsserter: AssertEqualsIgnoringOrder, assertOrder: true);
        AssertSql(
            """
SELECT s."JsonDictionary"::jsonb || s."JsonDictionary"::jsonb
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Dictionary_Concat_JsonbDictionary(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.Dictionary.Concat(s.JsonbDictionary)),
            elementAsserter: AssertEqualsIgnoringOrder, assertOrder: true);
        AssertSql(
            """
SELECT s."Dictionary" || (
    SELECT hstore(array_agg(j.key), array_agg(j.value))
    FROM jsonb_each_text(s."JsonbDictionary") AS j)
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Dictionary_Concat_JsonDictionary(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.Dictionary.Concat(s.JsonDictionary)),
            elementAsserter: AssertEqualsIgnoringOrder, assertOrder: true);
        AssertSql(
            """
SELECT s."Dictionary" || (
    SELECT hstore(array_agg(j.key), array_agg(j.value))
    FROM json_each_text(s."JsonDictionary") AS j)
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Dictionary_Keys_Concat_ImmutableDictionary_Keys(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.Dictionary.Keys.Concat(s.ImmutableDictionary.Keys)),
            elementAsserter: Assert.Equal, assertOrder: true);
        AssertSql(
            """
SELECT array_cat(akeys(s."Dictionary"), akeys(s."ImmutableDictionary"))
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ImmutableDictionary_Values_Concat_Dictionary_Values(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.ImmutableDictionary.Values.Concat(s.Dictionary.Values)),
            elementAsserter: Assert.Equal, assertOrder: true);
        AssertSql(
            """
SELECT array_cat(avals(s."ImmutableDictionary"), avals(s."Dictionary"))
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task ImmutableDictionary_Except_Dictionary(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.ImmutableDictionary.Except(s.Dictionary)),
            elementAsserter: AssertEqualsIgnoringOrder, assertOrder: true);
        AssertSql(
            """
SELECT s."ImmutableDictionary" - s."Dictionary"
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Dictionary_Except_ImmutableDictionary(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => s.Dictionary.Except(s.ImmutableDictionary)),
            elementAsserter: AssertEqualsIgnoringOrder, assertOrder: true);
        AssertSql(
            """
SELECT s."Dictionary" - s."ImmutableDictionary"
FROM "SomeEntities" AS s
""");
    }

    // [Theory]
    // [MemberData(nameof(IsAsyncData))]
    public async Task Extensions_ValuesForKeys(bool async)
    {
        string[] keys = ["key"];
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => keys.Select(key => s.Dictionary[key])),
            ss => ss.Set<DictionaryEntity>().Select(s
                => s.Dictionary.Where(d => keys.Contains(d.Key)).Select(d => d.Value).ToList()),
            elementAsserter: Assert.Equal, assertOrder: true);
        AssertSql(
            """
@__keys_1={ 'key' } (DbType = Object)

SELECT s."Dictionary" -> @__keys_1
FROM "SomeEntities" AS s
""");
    }

    // [Theory]
    // [MemberData(nameof(IsAsyncData))]
    public async Task Extensions_ValuesForKeys_JsonDictionary(bool async)
    {
        string[] keys = ["key"];
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => keys.Select(key => s.JsonDictionary[key])),
            ss => ss.Set<DictionaryEntity>().Select(
                s
                    // ReSharper disable once CanSimplifyDictionaryLookupWithTryGetValue
#pragma warning disable CA1854
                    => keys.Select(key => s.JsonDictionary.ContainsKey(key) ? s.JsonDictionary[key] : null)),
#pragma warning restore CA1854
            elementAsserter: Assert.Equal, assertOrder: true);
        AssertSql(
            """
@__keys_1={ 'key' } (DbType = Object)

SELECT (
    SELECT hstore(array_agg(j.key), array_agg(j.value))
    FROM json_each_text(s."JsonDictionary") AS j) -> @__keys_1
FROM "SomeEntities" AS s
""");
    }

    // [Theory]
    // [MemberData(nameof(IsAsyncData))]
    public async Task Extensions_ValuesForKeys_JsonbDictionary(bool async)
    {
        string[] keys = ["key"];
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => keys.Select(key => s.JsonbDictionary[key])),
            ss => ss.Set<DictionaryEntity>().Select(
                s
                    // ReSharper disable once CanSimplifyDictionaryLookupWithTryGetValue
#pragma warning disable CA1854
                    => keys.Select(key => s.JsonbDictionary.ContainsKey(key) ? s.JsonbDictionary[key] : null)),
#pragma warning restore CA1854
            elementAsserter: Assert.Equal, assertOrder: true);
        AssertSql(
            """
@__keys_1={ 'key' } (DbType = Object)

SELECT (
    SELECT hstore(array_agg(j.key), array_agg(j.value))
    FROM jsonb_each_text(s."JsonbDictionary") AS j) -> @__keys_1
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Extensions_FromJson(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => EF.Functions.ToHstore(s.JsonDictionary)),
            ss => ss.Set<DictionaryEntity>().Select(s => s.JsonDictionary),
            elementAsserter: AssertEqualsIgnoringOrder, assertOrder: true);
        AssertSql("""
SELECT (
    SELECT hstore(array_agg(j.key), array_agg(j.value))
    FROM json_each_text(s."JsonDictionary") AS j)
FROM "SomeEntities" AS s
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Extensions_ToHstore(bool async)
    {
        await AssertQuery(
            async, ss => ss.Set<DictionaryEntity>().Select(s => EF.Functions.ToHstore(s.JsonbDictionary)),
            ss => ss.Set<DictionaryEntity>().Select(s => s.JsonbDictionary.ToDictionary()),
            elementAsserter: AssertEqualsIgnoringOrder, assertOrder: true);
        AssertSql(
            """
SELECT (
    SELECT hstore(array_agg(j.key), array_agg(j.value))
    FROM jsonb_each_text(s."JsonbDictionary") AS j)
FROM "SomeEntities" AS s
""");
    }

    // ReSharper disable twice PossibleMultipleEnumeration
    private static void AssertEqualsIgnoringOrder<T>(IEnumerable<T> left, IEnumerable<T> right)
    {
        Console.WriteLine(left);
        Console.WriteLine(right);
        Assert.Empty(left.Except(right));
        Assert.Empty(right.Except(left));
    }

    protected void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
