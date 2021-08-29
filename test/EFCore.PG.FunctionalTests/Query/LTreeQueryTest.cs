using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    [MinimumPostgresVersion(13, 0)]
    public class LTreeQueryTest : IClassFixture<LTreeQueryTest.LTreeQueryFixture>
    {
        private LTreeQueryFixture Fixture { get; }

        // ReSharper disable once UnusedParameter.Local
        public LTreeQueryTest(LTreeQueryFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
            // Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalFact]
        public void Load()
        {
            using var ctx = CreateContext();
            var entity = ctx.LTreeEntities.Single(l => l.Id == 5);
            Assert.Equal("Top.Science.Astronomy.Cosmology", entity.Path);
            Assert.Equal("Top.Science.Astronomy.Cosmology", entity.PathAsString);
        }

        [ConditionalFact]
        public void Compare_to_string_literal()
        {
            using var ctx = CreateContext();
            var count = ctx.LTreeEntities.Count(l => l.Path == "Top.Science");

            Assert.Equal(1, count);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""LTreeEntities"" AS l
WHERE l.""Path"" = 'Top.Science'");
        }

        [ConditionalFact]
        public void Compare_to_string_parameter()
        {
            using var ctx = CreateContext();
            var p = "Top.Science";
            var count = ctx.LTreeEntities.Count(l => l.Path == p);

            Assert.Equal(1, count);
            AssertSql(
                @"@__p_0='Top.Science' (Nullable = false) (DbType = Object)

SELECT COUNT(*)::INT
FROM ""LTreeEntities"" AS l
WHERE l.""Path"" = @__p_0");
        }

        [ConditionalFact]
        public void Compare_string_to_string_literal()
        {
            using var ctx = CreateContext();
            var count = ctx.LTreeEntities.Count(l => l.PathAsString == "Top.Science");

            Assert.Equal(1, count);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""LTreeEntities"" AS l
WHERE l.""PathAsString"" = 'Top.Science'");
        }

        [ConditionalFact]
        public void LTree_cast_to_string()
        {
            using var ctx = CreateContext();
            var count = ctx.LTreeEntities.Count(l => ((string)l.Path).StartsWith("Top.Science"));

            Assert.Equal(4, count);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""LTreeEntities"" AS l
WHERE l.""Path""::text LIKE 'Top.Science%'");
        }

        [ConditionalFact]
        public void IsAncestorOf()
        {
            using var ctx = CreateContext();
            var count = ctx.LTreeEntities.Count(l => new LTree("Top.Science").IsAncestorOf(l.Path));

            Assert.Equal(4, count);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""LTreeEntities"" AS l
WHERE 'Top.Science' @> l.""Path""");
        }

        [ConditionalFact]
        public void IsDescendentOf()
        {
            using var ctx = CreateContext();
            var count = ctx.LTreeEntities.Count(l => l.Path.IsDescendantOf("Top.Science"));

            Assert.Equal(4, count);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""LTreeEntities"" AS l
WHERE l.""Path"" <@ 'Top.Science'");
        }

        [ConditionalFact]
        public void LTree_matches_LQuery()
        {
            using var ctx = CreateContext();
            var entity = ctx.LTreeEntities.Single(l => l.Path.MatchesLQuery("*.Astrophysics"));

            Assert.Equal(4, entity.Id);
            AssertSql(
                @"SELECT l.""Id"", l.""Path"", l.""PathAsString""
FROM ""LTreeEntities"" AS l
WHERE l.""Path"" ~ '*.Astrophysics'
LIMIT 2");
        }

        [ConditionalFact]
        public void LTree_matches_any_LQuery()
        {
            using var ctx = CreateContext();
            var lqueries = new[] { "*.Astrophysics", "*.Geology" };
            var entity = ctx.LTreeEntities.Single(l => lqueries.Any(q => l.Path.MatchesLQuery(q)));

            Assert.Equal(4, entity.Id);
            AssertSql(
                @"@__lqueries_0={ '*.Astrophysics', '*.Geology' } (DbType = Object)

SELECT l.""Id"", l.""Path"", l.""PathAsString""
FROM ""LTreeEntities"" AS l
WHERE l.""Path"" ? @__lqueries_0
LIMIT 2");
        }

        [ConditionalFact]
        public void LTree_matches_LTxtQuery()
        {
            using var ctx = CreateContext();
            var count = ctx.LTreeEntities.Count(l => l.Path.MatchesLTxtQuery("Astro*"));

            Assert.Equal(3, count);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""LTreeEntities"" AS l
WHERE l.""Path"" @ 'Astro*'");
        }

        [ConditionalFact]
        public void LTree_concat()
        {
            using var ctx = CreateContext();
            var entity = ctx.LTreeEntities.Single(l => l.Path + ".Astronomy" == "Top.Science.Astronomy");

            Assert.Equal(2, entity.Id);
            AssertSql(
                @"SELECT l.""Id"", l.""Path"", l.""PathAsString""
FROM ""LTreeEntities"" AS l
WHERE (l.""Path""::text || '.Astronomy') = 'Top.Science.Astronomy'
LIMIT 2");
        }

        [ConditionalFact]
        public void LTree_contains_any_LTree()
        {
            using var ctx = CreateContext();
            var ltrees = new LTree[] { "Top.Science", "Top.Art" };
            var count = ctx.LTreeEntities.Count(l => ltrees.Any(t => t.IsAncestorOf(l.Path)));

            Assert.Equal(4, count);
            AssertSql(
                @"@__ltrees_0={ 'Top.Science', 'Top.Art' } (DbType = Object)

SELECT COUNT(*)::INT
FROM ""LTreeEntities"" AS l
WHERE @__ltrees_0 @> l.""Path""");
        }

        [ConditionalFact]
        public void LTree_contained_by_any_LTree()
        {
            using var ctx = CreateContext();
            var ltrees = new LTree[] { "Top.Science.Astronomy", "Top.Art" };
            var count = ctx.LTreeEntities.Count(l => ltrees.Any(t => t.IsDescendantOf(l.Path)));

            Assert.Equal(3, count);
            AssertSql(
                @"@__ltrees_0={ 'Top.Science.Astronomy', 'Top.Art' } (DbType = Object)

SELECT COUNT(*)::INT
FROM ""LTreeEntities"" AS l
WHERE @__ltrees_0 <@ l.""Path""");
        }

        [ConditionalFact]
        public void Any_LTree_matches_LQuery()
        {
            using var ctx = CreateContext();

            var ltrees = new LTree[] { "Top.Science.Astronomy.Astrophysics", "Top.Science.Astronomy.Cosmology" };
            _ = ctx.LTreeEntities.Count(_ => ltrees.Any(t => t.MatchesLQuery("*.Astrophysics")));

            AssertSql(
                @"@__ltrees_0={ 'Top.Science.Astronomy.Astrophysics', 'Top.Science.Astronomy.Cosmology' } (DbType = Object)

SELECT COUNT(*)::INT
FROM ""LTreeEntities"" AS l
WHERE @__ltrees_0 ~ '*.Astrophysics'");
        }

        [ConditionalFact]
        public void Any_LTree_matches_any_LQuery()
        {
            using var ctx = CreateContext();

            var ltrees = new LTree[] { "Top.Science.Astronomy.Astrophysics", "Top.Science.Astronomy.Cosmology" };
            var lqueries = new[] { "*.Astrophysics", "*.Geology" };

            _ = ctx.LTreeEntities.Count(_ => ltrees.Any(t => lqueries.Any(q => t.MatchesLQuery(q))));

            AssertSql(
                @"@__ltrees_0={ 'Top.Science.Astronomy.Astrophysics', 'Top.Science.Astronomy.Cosmology' } (DbType = Object)
@__lqueries_1={ '*.Astrophysics', '*.Geology' } (DbType = Object)

SELECT COUNT(*)::INT
FROM ""LTreeEntities"" AS l
WHERE @__ltrees_0 ? @__lqueries_1");
        }

        [ConditionalFact]
        public void Any_LTree_matches_LTxtQuery()
        {
            using var ctx = CreateContext();

            var ltrees = new LTree[] { "Top.Science.Astronomy.Astrophysics", "Top.Science.Astronomy.Cosmology" };
            _ = ctx.LTreeEntities.Count(_ => ltrees.Any(t => t.MatchesLTxtQuery("Astro*")));

            AssertSql(
                @"@__ltrees_0={ 'Top.Science.Astronomy.Astrophysics', 'Top.Science.Astronomy.Cosmology' } (DbType = Object)

SELECT COUNT(*)::INT
FROM ""LTreeEntities"" AS l
WHERE @__ltrees_0 @ 'Astro*'");
        }

        [ConditionalFact]
        public void First_LTree_ancestor()
        {
            using var ctx = CreateContext();

            var ltrees = new LTree[] { "Top.Science", "Top.Hobbies" };
            var count = ctx.LTreeEntities.Count(l => ltrees.FirstOrDefault(l2 => l2.IsAncestorOf(l.Path)) == new LTree("Top.Science"));

            Assert.Equal(4, count);
            AssertSql(
                @"@__ltrees_0={ 'Top.Science', 'Top.Hobbies' } (DbType = Object)

SELECT COUNT(*)::INT
FROM ""LTreeEntities"" AS l
WHERE @__ltrees_0 ?@> l.""Path"" = 'Top.Science'");
        }

        [ConditionalFact]
        public void First_LTree_descendant()
        {
            using var ctx = CreateContext();

            var ltrees = new LTree[] { "Top.Science.Astronomy", "Top.Hobbies.Amateurs_Astronomy" };
            var count = ctx.LTreeEntities.Count(l => ltrees.FirstOrDefault(l2 => l2.IsDescendantOf(l.Path)) == "Top.Science.Astronomy");

            Assert.Equal(3, count);
            AssertSql(
                @"@__ltrees_0={ 'Top.Science.Astronomy', 'Top.Hobbies.Amateurs_Astronomy' } (DbType = Object)

SELECT COUNT(*)::INT
FROM ""LTreeEntities"" AS l
WHERE @__ltrees_0 ?<@ l.""Path"" = 'Top.Science.Astronomy'");
        }

        [ConditionalFact]
        public void First_LTree_matches_LQuery()
        {
            using var ctx = CreateContext();

            var ltrees = new LTree[] { "Top.Science.Astronomy.Astrophysics", "Top.Science.Astronomy.Cosmology" };
            _ = ctx.LTreeEntities.Count(_ => ltrees.FirstOrDefault(l => l.MatchesLQuery("*.Astrophysics")) == "Top.Science.Astronomy.Astrophysics");

            AssertSql(
                @"@__ltrees_0={ 'Top.Science.Astronomy.Astrophysics', 'Top.Science.Astronomy.Cosmology' } (DbType = Object)

SELECT COUNT(*)::INT
FROM ""LTreeEntities"" AS l
WHERE @__ltrees_0 ?~ '*.Astrophysics' = 'Top.Science.Astronomy.Astrophysics'");
        }

        [ConditionalFact]
        public void First_LTree_matches_LTxtQuery()
        {
            using var ctx = CreateContext();

            var ltrees = new LTree[] { "Top.Science.Astronomy.Astrophysics", "Top.Science.Astronomy.Cosmology" };
            _ = ctx.LTreeEntities.Count(_ => ltrees.FirstOrDefault(l => l.MatchesLTxtQuery("Astro*")) == "Top.Science.Astronomy.Astrophysics");

            AssertSql(
                @"@__ltrees_0={ 'Top.Science.Astronomy.Astrophysics', 'Top.Science.Astronomy.Cosmology' } (DbType = Object)

SELECT COUNT(*)::INT
FROM ""LTreeEntities"" AS l
WHERE @__ltrees_0 ?@ 'Astro*' = 'Top.Science.Astronomy.Astrophysics'");
        }

        [ConditionalFact]
        public void Subtree()
        {
            using var ctx = CreateContext();

            var count = ctx.LTreeEntities.Count(l => l.Path.Subtree(0, 1) == "Top");

            Assert.Equal(7, count);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""LTreeEntities"" AS l
WHERE subltree(l.""Path"", 0, 1) = 'Top'");
        }

        [ConditionalFact]
        public void Subpath1()
        {
            using var ctx = CreateContext();

            var count = ctx.LTreeEntities.Count(l => l.Path.Subpath(0, 2) == "Top.Science");

            Assert.Equal(4, count);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""LTreeEntities"" AS l
WHERE subpath(l.""Path"", 0, 2) = 'Top.Science'");
        }

        [ConditionalFact]
        public void Subpath2()
        {
            using var ctx = CreateContext();

            var result = ctx.LTreeEntities.Single(
                l => l.Path.NLevel > 2 && l.Path.Subpath(2) == "Astronomy.Astrophysics");

            Assert.Equal(4, result.Id);
            AssertSql(
                @"SELECT l.""Id"", l.""Path"", l.""PathAsString""
FROM ""LTreeEntities"" AS l
WHERE (nlevel(l.""Path"") > 2) AND (subpath(l.""Path"", 2) = 'Astronomy.Astrophysics')
LIMIT 2");
        }

        [ConditionalFact]
        public void NLevel()
        {
            using var ctx = CreateContext();

            var count = ctx.LTreeEntities.Count(l => l.Path.NLevel == 2);

            Assert.Equal(2, count);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""LTreeEntities"" AS l
WHERE nlevel(l.""Path"") = 2");
        }

        [ConditionalFact]
        public void Index1()
        {
            using var ctx = CreateContext();

            var count = ctx.LTreeEntities.Count(l => l.Path.Index("Astronomy") != -1);

            Assert.Equal(3, count);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""LTreeEntities"" AS l
WHERE index(l.""Path"", 'Astronomy') <> -1");
        }

        [ConditionalFact]
        public void Index2()
        {
            using var ctx = CreateContext();

            var count = ctx.LTreeEntities.Count(l => l.Path.Index("Top", 1) != -1);

            Assert.Equal(0, count);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""LTreeEntities"" AS l
WHERE index(l.""Path"", 'Top', 1) <> -1");
        }

        [ConditionalFact]
        public void LongestCommonAncestor()
        {
            using var ctx = CreateContext();

            var count = ctx.LTreeEntities.Count(l => LTree.LongestCommonAncestor(l.Path, "Top.Hobbies") == "Top");

            Assert.Equal(6, count);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""LTreeEntities"" AS l
WHERE lca(ARRAY[l.""Path"",'Top.Hobbies']::ltree[]) = 'Top'");
        }

        #region Support

        protected LTreeQueryContext CreateContext() => Fixture.CreateContext();

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class LTreeQueryContext : PoolableDbContext
        {
            public DbSet<LTreeEntity> LTreeEntities { get; set; }

            public LTreeQueryContext(DbContextOptions options) : base(options) {}

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.HasPostgresExtension("ltree");

            public static void Seed(LTreeQueryContext context)
            {
                var ltreeEntities = new LTreeEntity[]
                {
                    new() { Id = 1, Path = "Top" },
                    new() { Id = 2, Path = "Top.Science" },
                    new() { Id = 3, Path = "Top.Science.Astronomy" },
                    new() { Id = 4, Path = "Top.Science.Astronomy.Astrophysics" },
                    new() { Id = 5, Path = "Top.Science.Astronomy.Cosmology" },
                    new() { Id = 6, Path = "Top.Hobbies" },
                    new() { Id = 7, Path = "Top.Hobbies.Amateurs_Astronomy" }
                };

                foreach (var ltreeEntity in ltreeEntities)
                {
                    ltreeEntity.PathAsString = ltreeEntity.Path;
                }

                context.LTreeEntities.AddRange(ltreeEntities);
                context.SaveChanges();
            }
        }

        public class LTreeEntity
        {
            public int Id { get; set; }

            [Required]
            public LTree Path { get; set; }

            [Required]
            [Column(TypeName = "ltree")]
            public string PathAsString { get; set; }
        }

        public class LTreeQueryFixture : SharedStoreFixtureBase<LTreeQueryContext>
        {
            protected override string StoreName => "LTreeQueryTest";
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
            protected override void Seed(LTreeQueryContext context) => LTreeQueryContext.Seed(context);
            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                => modelBuilder.HasPostgresExtension("ltree");
        }

        #endregion
    }
}
