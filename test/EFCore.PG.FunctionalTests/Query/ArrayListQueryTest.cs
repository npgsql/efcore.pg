using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class ArrayListQueryTest : IClassFixture<ArrayListQueryTest.ArrayListQueryFixture>
    {
        ArrayListQueryFixture Fixture { get; }

        public ArrayListQueryTest(ArrayListQueryFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
            Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        #region Roundtrip

        [Fact]
        public void Roundtrip()
        {
            using var ctx = CreateContext();
            var x = ctx.SomeEntities.Single(e => e.Id == 1);

            Assert.Equal(new List<int> { 3, 4 }, x.SomeList);
        }

        #endregion

        #region Indexers

        [Fact]
        public void Index_with_constant()
        {
            using var ctx = CreateContext();
            var actual = ctx.SomeEntities.Where(e => e.SomeList[0] == 3).ToList();

            Assert.Single(actual);
            AssertSql(
                @"SELECT s.""Id"", s.""SomeList"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE s.""SomeList""[1] = 3");
        }

        [Fact]
        public void Index_with_non_constant()
        {
            using var ctx = CreateContext();
            // ReSharper disable once ConvertToConstant.Local
            var x = 0;
            var actual = ctx.SomeEntities.Where(e => e.SomeList[x] == 3).ToList();

            Assert.Single(actual);
            AssertSql(
                @"@__x_0='0'

SELECT s.""Id"", s.""SomeList"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE s.""SomeList""[@__x_0 + 1] = 3");
        }

        #endregion

        #region Equality

        [Fact]
        public void SequenceEqual_with_parameter()
        {
            using var ctx = CreateContext();
            var arr = new[] { 3, 4 };
            var x = ctx.SomeEntities.Single(e => e.SomeList.SequenceEqual(arr));

            Assert.Equal(new[] { 3, 4 }, x.SomeList);
            AssertSql(
                @"@__arr_0='System.Int32[]' (DbType = Object)

SELECT s.""Id"", s.""SomeList"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE s.""SomeList"" = @__arr_0
LIMIT 2");
        }

        [Fact]
        public void SequenceEqual_with_array_literal()
        {
            using var ctx = CreateContext();
            var x = ctx.SomeEntities.Single(e => e.SomeList.SequenceEqual(new[] { 3, 4 }));

            Assert.Equal(new[] { 3, 4 }, x.SomeList);
            AssertSql(
                @"SELECT s.""Id"", s.""SomeList"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE s.""SomeList"" = ARRAY[3,4]::integer[]
LIMIT 2");
        }

        #endregion

        #region Containment

        [Fact]
        public void Contains_with_literal()
        {
            using var ctx = CreateContext();
            var x = ctx.SomeEntities.Single(e => e.SomeList.Contains(3));

            Assert.Equal(new[] { 3, 4 }, x.SomeList);
            AssertSql(
                @"SELECT s.""Id"", s.""SomeList"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE 3 = ANY (s.""SomeList"")
LIMIT 2");
        }

        [Fact]
        public void Contains_with_parameter()
        {
            using var ctx = CreateContext();
            // ReSharper disable once ConvertToConstant.Local
            var p = 3;
            var x = ctx.SomeEntities.Single(e => e.SomeList.Contains(p));

            Assert.Equal(new[] { 3, 4 }, x.SomeList);
            AssertSql(
                @"@__p_0='3'

SELECT s.""Id"", s.""SomeList"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE @__p_0 = ANY (s.""SomeList"")
LIMIT 2");
        }

        [Fact]
        public void Contains_with_column()
        {
            using var ctx = CreateContext();
            var x = ctx.SomeEntities.Single(e => e.SomeList.Contains(e.Id + 2));

            Assert.Equal(new[] { 3, 4 }, x.SomeList);
            AssertSql(
                @"SELECT s.""Id"", s.""SomeList"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE s.""Id"" + 2 = ANY (s.""SomeList"")
LIMIT 2");
        }

        #endregion

        #region Count

        [Fact(Skip = "https://github.com/dotnet/efcore/issues/19715")]
        public void Count()
        {
            using var ctx = CreateContext();
            var x = ctx.SomeEntities.Single(e => e.SomeList.Count == 2);

            Assert.Equal(new[] { 3, 4 }, x.SomeList);
            AssertSql(
                @"SELECT s.""Id"", s.""SomeList"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE cardinality(s.""SomeList"") = 2
LIMIT 2");
        }

        [Fact]
        public void Count_on_EF_Property()
        {
            using var ctx = CreateContext();
            var x = ctx.SomeEntities.Single(e => EF.Property<int[]>(e, nameof(SomeListEntity.SomeList)).Length == 2);

            Assert.Equal(new[] { 3, 4 }, x.SomeList);
            AssertSql(
                @"SELECT s.""Id"", s.""SomeList"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE cardinality(s.""SomeList"") = 2
LIMIT 2");
        }

        [Fact]
        public void Count_on_literal_not_translated()
        {
            using var ctx = CreateContext();
            var _ = ctx.SomeEntities.Where(e => new List<int> { 1, 2, 3 }.Count == e.Id).ToList();

            AssertDoesNotContainInSql("cardinality");
        }

        #endregion

        #region AnyAll

        [Fact]
        public void Any_no_predicate()
        {
            using var ctx = CreateContext();
            var count = ctx.SomeEntities.Count(e => e.SomeList.Any());

            Assert.Equal(2, count);
            AssertSql(
                @"SELECT COUNT(*)::INT
FROM ""SomeEntities"" AS s
WHERE cardinality(s.""SomeList"") > 0");
        }

        [Fact]
        public void Any_like()
        {
            using var ctx = CreateContext();
            var _ = ctx.SomeEntities
                .Where(e => new List<string> { "a%", "b%", "c%" }.Any(p => EF.Functions.Like(e.SomeText, p)))
                .ToList();

            AssertSql(
                @"SELECT s.""Id"", s.""SomeList"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE s.""SomeText"" LIKE ANY (ARRAY['a%','b%','c%']::text[])");
        }

        [Fact]
        public void Any_ilike()
        {
            using var ctx = CreateContext();
            var _ = ctx.SomeEntities
                .Where(e => new[] { "a%", "b%", "c%" }.Any(p => EF.Functions.ILike(e.SomeText, p)))
                .ToList();

            AssertSql(
                @"SELECT s.""Id"", s.""SomeList"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE s.""SomeText"" ILIKE ANY (ARRAY['a%','b%','c%']::text[])");
        }

        [Fact]
        public void Any_like_anonymous()
        {
            using var ctx = CreateContext();
            var patterns = new[] { "a%", "b%", "c%" };

            var anon =
                ctx.SomeEntities
                    .Select(
                        x => new
                        {
                            List = x.SomeList,
                            Text = x.SomeText
                        });

            var _ = anon.Where(x => patterns.Any(p => EF.Functions.Like(x.Text, p))).ToList();

            AssertSql(
                @"@__patterns_0='System.String[]' (DbType = Object)

SELECT s.""SomeList"" AS ""List"", s.""SomeText"" AS ""Text""
FROM ""SomeEntities"" AS s
WHERE s.""SomeText"" LIKE ANY (@__patterns_0)");
        }

        [Fact]
        public void Any_Contains()
        {
            using var ctx = CreateContext();

            var results = ctx.SomeEntities
                .Where(e => new[] { 2, 3 }.Any(p => e.SomeList.Contains(p)))
                .ToList();
            Assert.Equal(1, Assert.Single(results).Id);

            results = ctx.SomeEntities
                .Where(e => new[] { 1, 2 }.Any(p => e.SomeList.Contains(p)))
                .ToList();
            Assert.Empty(results);

            AssertSql(
                @"SELECT s.""Id"", s.""SomeList"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE (ARRAY[2,3]::integer[] && s.""SomeList"")",
                //
                @"SELECT s.""Id"", s.""SomeList"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE (ARRAY[1,2]::integer[] && s.""SomeList"")");
        }

        [Fact]
        public void All_Contains()
        {
            using var ctx = CreateContext();

            var results = ctx.SomeEntities
                .Where(e => new[] { 5, 6 }.All(p => e.SomeList.Contains(p)))
                .ToList();
            Assert.Equal(2, Assert.Single(results).Id);

            results = ctx.SomeEntities
                .Where(e => new[] { 4, 5, 6 }.All(p => e.SomeList.Contains(p)))
                .ToList();
            Assert.Empty(results);

            AssertSql(
                @"SELECT s.""Id"", s.""SomeList"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE (ARRAY[5,6]::integer[] <@ s.""SomeList"")",
                //
                @"SELECT s.""Id"", s.""SomeList"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE (ARRAY[4,5,6]::integer[] <@ s.""SomeList"")");
        }

        #endregion

        #region Support

        protected ArrayListQueryContext CreateContext() => Fixture.CreateContext();

        void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        void AssertDoesNotContainInSql(string expected)
            => Assert.DoesNotContain(expected, Fixture.TestSqlLoggerFactory.Sql);

        public class ArrayListQueryContext : PoolableDbContext
        {
            public DbSet<SomeListEntity> SomeEntities { get; set; }

            public ArrayListQueryContext(DbContextOptions options) : base(options) {}

            public static void Seed(ArrayListQueryContext context)
            {
                context.SomeEntities.AddRange(
                    new SomeListEntity
                    {
                        Id = 1,
                        SomeList = new List<int> { 3, 4 },
                        SomeText = "foo"
                    },
                    new SomeListEntity
                    {
                        Id = 2,
                        SomeList = new List<int> { 5, 6, 7 },
                        SomeText = "bar"
                    });
                context.SaveChanges();
            }
        }

        public class SomeListEntity
        {
            public int Id { get; set; }
            public List<int> SomeList { get; set; }
            public string SomeText { get; set; }
        }

        public class ArrayListQueryFixture : SharedStoreFixtureBase<ArrayListQueryContext>
        {
            protected override string StoreName => "ArrayListQueryTest";
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
            protected override void Seed(ArrayListQueryContext context) => ArrayListQueryContext.Seed(context);
        }

        #endregion
    }
}
