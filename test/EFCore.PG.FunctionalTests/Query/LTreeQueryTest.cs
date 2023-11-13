using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

[MinimumPostgresVersion(13, 0)]
public class LTreeQueryTest : IClassFixture<LTreeQueryTest.LTreeQueryFixture>
{
    private LTreeQueryFixture Fixture { get; }

    // ReSharper disable once UnusedParameter.Local
    public LTreeQueryTest(LTreeQueryFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public void Load()
    {
        using var ctx = CreateContext();
        var entity = ctx.LTreeEntities.Single(l => l.Id == 5);
        Assert.Equal("Top.Science.Astronomy.Cosmology", entity.LTree);
        Assert.Equal("Top.Science.Astronomy.Cosmology", entity.LTreeAsString);
    }

    [ConditionalFact]
    public void Compare_to_string_literal()
    {
        using var ctx = CreateContext();
        var count = ctx.LTreeEntities.Count(l => l.LTree == "Top.Science");

        Assert.Equal(1, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE l."LTree" = 'Top.Science'
""");
    }

    [ConditionalFact]
    public void Compare_to_string_parameter()
    {
        using var ctx = CreateContext();
        var p = "Top.Science";
        var count = ctx.LTreeEntities.Count(l => l.LTree == p);

        Assert.Equal(1, count);
        AssertSql(
            """
@__p_0='Top.Science' (Nullable = false) (DbType = Object)

SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE l."LTree" = @__p_0
""");
    }

    [ConditionalFact]
    public void Compare_string_to_string_literal()
    {
        using var ctx = CreateContext();
        var count = ctx.LTreeEntities.Count(l => l.LTreeAsString == "Top.Science");

        Assert.Equal(1, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE l."LTreeAsString" = 'Top.Science'
""");
    }

    [ConditionalFact]
    public void LTree_cast_to_string()
    {
        using var ctx = CreateContext();
        var count = ctx.LTreeEntities.Count(l => ((string)l.LTree).StartsWith("Top.Science"));

        Assert.Equal(4, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE l."LTree"::text LIKE 'Top.Science%'
""");
    }

    [ConditionalFact]
    public void IsAncestorOf()
    {
        using var ctx = CreateContext();
        var count = ctx.LTreeEntities.Count(l => new LTree("Top.Science").IsAncestorOf(l.LTree));

        Assert.Equal(4, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE 'Top.Science' @> l."LTree"
""");
    }

    [ConditionalFact]
    public void IsDescendentOf()
    {
        using var ctx = CreateContext();
        var count = ctx.LTreeEntities.Count(l => l.LTree.IsDescendantOf("Top.Science"));

        Assert.Equal(4, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE l."LTree" <@ 'Top.Science'
""");
    }

    [ConditionalFact]
    public void LTree_matches_LQuery()
    {
        using var ctx = CreateContext();
        var entity = ctx.LTreeEntities.Single(l => l.LTree.MatchesLQuery("*.Astrophysics"));

        Assert.Equal(4, entity.Id);
        AssertSql(
            """
SELECT l."Id", l."LTree", l."LTreeAsString", l."LTrees", l."SomeString"
FROM "LTreeEntities" AS l
WHERE l."LTree" ~ '*.Astrophysics'
LIMIT 2
""");
    }

    [ConditionalFact] // #2487
    public void LTree_matches_LQuery_with_string_column()
    {
        using var ctx = CreateContext();
        var entity = ctx.LTreeEntities.Single(l => l.LTree.MatchesLQuery(l.SomeString));

        Assert.Equal(4, entity.Id);
        AssertSql(
            """
SELECT l."Id", l."LTree", l."LTreeAsString", l."LTrees", l."SomeString"
FROM "LTreeEntities" AS l
WHERE l."LTree" ~ l."SomeString"::lquery
LIMIT 2
""");
    }

    [ConditionalFact] // #2487
    public void LTree_matches_LQuery_with_concat()
    {
        using var ctx = CreateContext();
        var count = ctx.LTreeEntities.Count(l => l.LTree.MatchesLQuery("*.Astrophysics." + l.Id));

        Assert.Equal(0, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE l."LTree" ~ CAST('*.Astrophysics.' || l."Id"::text AS lquery)
""");
    }

    [ConditionalFact]
    public void LTree_matches_any_LQuery()
    {
        using var ctx = CreateContext();
        var lqueries = new[] { "*.Astrophysics", "*.Geology" };
        var entity = ctx.LTreeEntities.Single(l => lqueries.Any(q => l.LTree.MatchesLQuery(q)));

        Assert.Equal(4, entity.Id);

        AssertSql(
            """
@__lqueries_0={ '*.Astrophysics', '*.Geology' } (DbType = Object)

SELECT l."Id", l."LTree", l."LTreeAsString", l."LTrees", l."SomeString"
FROM "LTreeEntities" AS l
WHERE l."LTree" ? @__lqueries_0
LIMIT 2
""");
    }

    [ConditionalFact]
    public void LTree_matches_LTxtQuery()
    {
        using var ctx = CreateContext();
        var count = ctx.LTreeEntities.Count(l => l.LTree.MatchesLTxtQuery("Astro*"));

        Assert.Equal(3, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE l."LTree" @ 'Astro*'
""");
    }

    [ConditionalFact]
    public void LTree_concat()
    {
        using var ctx = CreateContext();
        var entity = ctx.LTreeEntities.Single(l => l.LTree + ".Astronomy" == "Top.Science.Astronomy");

        Assert.Equal(2, entity.Id);
        AssertSql(
            """
SELECT l."Id", l."LTree", l."LTreeAsString", l."LTrees", l."SomeString"
FROM "LTreeEntities" AS l
WHERE l."LTree"::text || '.Astronomy' = 'Top.Science.Astronomy'
LIMIT 2
""");
    }

    [ConditionalFact]
    public void LTree_contains_any_LTree()
    {
        using var ctx = CreateContext();
        var ltrees = new LTree[] { "Top.Science", "Top.Art" };
        var count = ctx.LTreeEntities.Count(l => ltrees.Any(t => t.IsAncestorOf(l.LTree)));

        Assert.Equal(4, count);
        AssertSql(
            """
@__ltrees_0={ 'Top.Science', 'Top.Art' } (DbType = Object)

SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE @__ltrees_0 @> l."LTree"
""");
    }

    [ConditionalFact]
    public void LTree_contained_by_any_LTree()
    {
        using var ctx = CreateContext();
        var ltrees = new LTree[] { "Top.Science.Astronomy", "Top.Art" };
        var count = ctx.LTreeEntities.Count(l => ltrees.Any(t => t.IsDescendantOf(l.LTree)));

        Assert.Equal(3, count);
        AssertSql(
            """
@__ltrees_0={ 'Top.Science.Astronomy', 'Top.Art' } (DbType = Object)

SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE @__ltrees_0 <@ l."LTree"
""");
    }

    [ConditionalFact]
    public void Any_LTree_matches_LQuery()
    {
        using var ctx = CreateContext();

        var ltrees = new LTree[] { "Top.Science.Astronomy.Astrophysics", "Top.Science.Astronomy.Cosmology" };
        _ = ctx.LTreeEntities.Count(_ => ltrees.Any(t => t.MatchesLQuery("*.Astrophysics")));

        AssertSql(
            """
@__ltrees_0={ 'Top.Science.Astronomy.Astrophysics', 'Top.Science.Astronomy.Cosmology' } (DbType = Object)

SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE @__ltrees_0 ~ '*.Astrophysics'
""");
    }

    [ConditionalFact]
    public void Any_LTree_matches_any_LQuery()
    {
        using var ctx = CreateContext();

        var lqueries = new[] { "*.Astrophysics", "*.Geology" };

        _ = ctx.LTreeEntities.Count(l => l.LTrees.Any(t => lqueries.Any(q => t.MatchesLQuery(q))));

        AssertSql(
            """
@__lqueries_0={ '*.Astrophysics', '*.Geology' } (DbType = Object)

SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE l."LTrees" ? @__lqueries_0
""");
    }

    [ConditionalFact]
    public void Any_LTree_matches_LTxtQuery()
    {
        using var ctx = CreateContext();

        _ = ctx.LTreeEntities.Count(l => l.LTrees.Any(t => t.MatchesLTxtQuery("Astro*")));

        AssertSql(
            """
SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE l."LTrees" @ 'Astro*'
""");
    }

    [ConditionalFact]
    public void First_LTree_ancestor()
    {
        using var ctx = CreateContext();

        var ltrees = new LTree[] { "Top.Science", "Top.Hobbies" };
        var count = ctx.LTreeEntities.Count(l => ltrees.FirstOrDefault(l2 => l2.IsAncestorOf(l.LTree)) == new LTree("Top.Science"));

        Assert.Equal(4, count);
        AssertSql(
            """
@__ltrees_0={ 'Top.Science', 'Top.Hobbies' } (DbType = Object)

SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE @__ltrees_0 ?@> l."LTree" = 'Top.Science'
""");
    }

    [ConditionalFact]
    public void First_LTree_descendant()
    {
        using var ctx = CreateContext();

        var ltrees = new LTree[] { "Top.Science.Astronomy", "Top.Hobbies.Amateurs_Astronomy" };
        var count = ctx.LTreeEntities.Count(l => ltrees.FirstOrDefault(l2 => l2.IsDescendantOf(l.LTree)) == "Top.Science.Astronomy");

        Assert.Equal(3, count);
        AssertSql(
            """
@__ltrees_0={ 'Top.Science.Astronomy', 'Top.Hobbies.Amateurs_Astronomy' } (DbType = Object)

SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE @__ltrees_0 ?<@ l."LTree" = 'Top.Science.Astronomy'
""");
    }

    [ConditionalFact]
    public void First_LTree_matches_LQuery()
    {
        using var ctx = CreateContext();

        var ltrees = new LTree[] { "Top.Science.Astronomy.Astrophysics", "Top.Science.Astronomy.Cosmology" };
        _ = ctx.LTreeEntities.Count(
            _ => ltrees.FirstOrDefault(l => l.MatchesLQuery("*.Astrophysics")) == "Top.Science.Astronomy.Astrophysics");

        AssertSql(
            """
@__ltrees_0={ 'Top.Science.Astronomy.Astrophysics', 'Top.Science.Astronomy.Cosmology' } (DbType = Object)

SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE @__ltrees_0 ?~ '*.Astrophysics' = 'Top.Science.Astronomy.Astrophysics'
""");
    }

    [ConditionalFact]
    public void First_LTree_matches_LTxtQuery()
    {
        using var ctx = CreateContext();

        var ltrees = new LTree[] { "Top.Science.Astronomy.Astrophysics", "Top.Science.Astronomy.Cosmology" };
        _ = ctx.LTreeEntities.Count(_ => ltrees.FirstOrDefault(l => l.MatchesLTxtQuery("Astro*")) == "Top.Science.Astronomy.Astrophysics");

        AssertSql(
            """
@__ltrees_0={ 'Top.Science.Astronomy.Astrophysics', 'Top.Science.Astronomy.Cosmology' } (DbType = Object)

SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE @__ltrees_0 ?@ 'Astro*' = 'Top.Science.Astronomy.Astrophysics'
""");
    }

    [ConditionalFact]
    public void Subtree()
    {
        using var ctx = CreateContext();

        var count = ctx.LTreeEntities.Count(l => l.LTree.Subtree(0, 1) == "Top");

        Assert.Equal(7, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE subltree(l."LTree", 0, 1) = 'Top'
""");
    }

    [ConditionalFact]
    public void Subpath1()
    {
        using var ctx = CreateContext();

        var count = ctx.LTreeEntities.Count(l => l.LTree.Subpath(0, 2) == "Top.Science");

        Assert.Equal(4, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE subpath(l."LTree", 0, 2) = 'Top.Science'
""");
    }

    [ConditionalFact]
    public void Subpath2()
    {
        using var ctx = CreateContext();

        var result = ctx.LTreeEntities.Single(
            l => l.LTree.NLevel > 2 && l.LTree.Subpath(2) == "Astronomy.Astrophysics");

        Assert.Equal(4, result.Id);
        AssertSql(
            """
SELECT l."Id", l."LTree", l."LTreeAsString", l."LTrees", l."SomeString"
FROM "LTreeEntities" AS l
WHERE nlevel(l."LTree") > 2 AND subpath(l."LTree", 2) = 'Astronomy.Astrophysics'
LIMIT 2
""");
    }

    [ConditionalFact]
    public void NLevel()
    {
        using var ctx = CreateContext();

        var count = ctx.LTreeEntities.Count(l => l.LTree.NLevel == 2);

        Assert.Equal(2, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE nlevel(l."LTree") = 2
""");
    }

    [ConditionalFact]
    public void Index1()
    {
        using var ctx = CreateContext();

        var count = ctx.LTreeEntities.Count(l => l.LTree.Index("Astronomy") != -1);

        Assert.Equal(3, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE index(l."LTree", 'Astronomy') <> -1
""");
    }

    [ConditionalFact]
    public void Index2()
    {
        using var ctx = CreateContext();

        var count = ctx.LTreeEntities.Count(l => l.LTree.Index("Top", 1) != -1);

        Assert.Equal(0, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE index(l."LTree", 'Top', 1) <> -1
""");
    }

    [ConditionalFact]
    public void LongestCommonAncestor()
    {
        using var ctx = CreateContext();

        var count = ctx.LTreeEntities.Count(l => LTree.LongestCommonAncestor(l.LTree, "Top.Hobbies") == "Top");

        Assert.Equal(6, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "LTreeEntities" AS l
WHERE lca(ARRAY[l."LTree",'Top.Hobbies']::ltree[]) = 'Top'
""");
    }

    #region Support

    protected LTreeQueryContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class LTreeQueryContext : PoolableDbContext
    {
        public DbSet<LTreeEntity> LTreeEntities { get; set; }

        public LTreeQueryContext(DbContextOptions options)
            : base(options)
        {
        }

        public static void Seed(LTreeQueryContext context)
        {
            var ltreeEntities = new LTreeEntity[]
            {
                new() { Id = 1, LTree = "Top" },
                new() { Id = 2, LTree = "Top.Science" },
                new() { Id = 3, LTree = "Top.Science.Astronomy" },
                new() { Id = 4, LTree = "Top.Science.Astronomy.Astrophysics" },
                new() { Id = 5, LTree = "Top.Science.Astronomy.Cosmology" },
                new() { Id = 6, LTree = "Top.Hobbies" },
                new() { Id = 7, LTree = "Top.Hobbies.Amateurs_Astronomy" }
            };

            foreach (var ltreeEntity in ltreeEntities)
            {
                ltreeEntity.LTreeAsString = ltreeEntity.LTree;
                ltreeEntity.SomeString = "*.Astrophysics";
                ltreeEntity.LTrees = new LTree[] { ltreeEntity.LTree, "Foo" };
            }

            context.LTreeEntities.AddRange(ltreeEntities);
            context.SaveChanges();
        }
    }

    public class LTreeEntity
    {
        public int Id { get; set; }

        [Required]
        public LTree LTree { get; set; }

        [Required]
        public LTree[] LTrees { get; set; }

        [Required]
        [Column(TypeName = "ltree")]
        public string LTreeAsString { get; set; }

        [Required]
        public string SomeString { get; set; }
    }

    public class LTreeQueryFixture : SharedStoreFixtureBase<LTreeQueryContext>
    {
        protected override string StoreName
            => "LTreeQueryTest";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override void Seed(LTreeQueryContext context)
            => LTreeQueryContext.Seed(context);
    }

    #endregion
}
