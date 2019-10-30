using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class ArrayQueryTest : IClassFixture<ArrayQueryTest.ArrayQueryFixture>
    {
        ArrayQueryFixture Fixture { get; }

        public ArrayQueryTest(ArrayQueryFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        #region Roundtrip

        [Fact]
        public void Roundtrip()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.Id == 1);
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);
                Assert.Equal(new List<int> { 3, 4 }, x.SomeList);
            }
        }

        #endregion

        #region Indexers

        [Fact]
        public void Index_with_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var actual = ctx.SomeEntities.Where(e => e.SomeArray[0] == 3).ToList();
                Assert.Single(actual);

                AssertSql(
                    @"SELECT s.""Id"", s.""SomeArray"", s.""SomeBytea"", s.""SomeList"", s.""SomeMatrix"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE s.""SomeArray""[1] = 3");
            }
        }

        [Fact]
        public void Index_with_non_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var x = 0;
                var actual = ctx.SomeEntities.Where(e => e.SomeArray[x] == 3).ToList();
                Assert.Single(actual);

                AssertSql(
                    @"@__x_0='0'

SELECT s.""Id"", s.""SomeArray"", s.""SomeBytea"", s.""SomeList"", s.""SomeMatrix"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE (s.""SomeArray""[@__x_0 + 1] = 3) AND (s.""SomeArray""[@__x_0 + 1] IS NOT NULL)");
            }
        }

        [Fact]
        public void Index_bytea_with_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var actual = ctx.SomeEntities.Where(e => e.SomeBytea[0] == 3).ToList();
                Assert.Single(actual);

                AssertSql(
                    @"SELECT s.""Id"", s.""SomeArray"", s.""SomeBytea"", s.""SomeList"", s.""SomeMatrix"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE (get_byte(s.""SomeBytea"", 0) = 3) AND (get_byte(s.""SomeBytea"", 0) IS NOT NULL)");
            }
        }

        [Fact(Skip = "Disabled since EF Core 3.0")]
        public void Index_text_with_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var actual = ctx.SomeEntities.Where(e => e.SomeText[0] == 'f').ToList();
                Assert.Single(actual);

                AssertSql(
                    @"SELECT s.""Id"", s.""SomeArray"", s.""SomeBytea"", s.""SomeList"", s.""SomeMatrix"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE (get_byte(s.""SomeBytea"", 0) = 3) AND get_byte(s.""SomeBytea"", 0) IS NOT NULL");
            }
        }

        #endregion

        #region Equality

        [Fact]
        public void SequenceEqual_with_parameter()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var arr = new[] { 3, 4 };
                var x = ctx.SomeEntities.Single(e => e.SomeArray.SequenceEqual(arr));
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);

                AssertSql(
                    @"@__arr_0='System.Int32[]' (DbType = Object)

SELECT s.""Id"", s.""SomeArray"", s.""SomeBytea"", s.""SomeList"", s.""SomeMatrix"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE (s.""SomeArray"" = @__arr_0) AND (s.""SomeArray"" IS NOT NULL)
LIMIT 2");
            }
        }

        [Fact]
        public void SequenceEqual_with_array_literal()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.SomeArray.SequenceEqual(new[] { 3, 4 }));
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);

                AssertSql(
                    @"SELECT s.""Id"", s.""SomeArray"", s.""SomeBytea"", s.""SomeList"", s.""SomeMatrix"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE (s.""SomeArray"" = ARRAY[3,4]::integer[]) AND (s.""SomeArray"" IS NOT NULL)
LIMIT 2");            }
        }

        #endregion

        #region Containment

        [Fact(Skip = "https://github.com/aspnet/EntityFrameworkCore/issues/17374")]
        public void Contains_with_literal()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.SomeArray.Contains(3));
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);

                AssertSql(
                    @"SELECT s.""Id"", s.""SomeArray"", s.""SomeBytea"", s.""SomeList"", s.""SomeMatrix"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE 3 = ANY (s.""SomeArray"")
LIMIT 2");
            }
        }

        [Fact(Skip = "https://github.com/aspnet/EntityFrameworkCore/issues/17374")]
        public void Contains_with_parameter()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var p = 3;
                var x = ctx.SomeEntities.Single(e => e.SomeArray.Contains(p));
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);

                AssertSql(
                    @"@__p_0='3'

SELECT s.""Id"", s.""SomeArray"", s.""SomeBytea"", s.""SomeList"", s.""SomeMatrix"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE @__p_0 = ANY (s.""SomeArray"")
LIMIT 2");
            }
        }

        [Fact(Skip = "https://github.com/aspnet/EntityFrameworkCore/issues/17374")]
        public void Contains_with_column()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.SomeArray.Contains(e.Id + 2));
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);

                AssertSql(
                    @"SELECT s.""Id"", s.""SomeArray"", s.""SomeBytea"", s.""SomeList"", s.""SomeMatrix"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE s.""Id"" + 2 = ANY (s.""SomeArray"")
LIMIT 2");
            }
        }

        #endregion

        #region Length

        [Fact]
        public void Length()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.SomeArray.Length == 2);
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);

                AssertSql(
                    @"SELECT s.""Id"", s.""SomeArray"", s.""SomeBytea"", s.""SomeList"", s.""SomeMatrix"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE (cardinality(s.""SomeArray"") = 2) AND (cardinality(s.""SomeArray"") IS NOT NULL)
LIMIT 2");            }
        }

        [Fact]
        public void Length_on_EF_Property()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => EF.Property<int[]>(e, nameof(SomeArrayEntity.SomeArray)).Length == 2);
                Assert.Equal(new[] { 3, 4 }, x.SomeArray);

                AssertSql(
                    @"SELECT s.""Id"", s.""SomeArray"", s.""SomeBytea"", s.""SomeList"", s.""SomeMatrix"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE (cardinality(s.""SomeArray"") = 2) AND (cardinality(s.""SomeArray"") IS NOT NULL)
LIMIT 2");            }
        }

        [Fact]
        public void Length_on_literal_not_translated()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => new[] { 1, 2, 3 }.Length == e.Id).ToList();
                AssertDoesNotContainInSql("cardinality");
            }
        }

        #endregion

        #region AnyAll

        [Fact(Skip = "https://github.com/aspnet/EntityFrameworkCore/issues/17374")]
        public void Any_no_predicate()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var count = ctx.SomeEntities.Count(e => e.SomeArray.Any());
                Assert.Equal(2, count);

                AssertSql(
                    @"SELECT COUNT(*)::INT
FROM ""SomeEntities"" AS s
WHERE cardinality(s.""SomeArray"") > 0");
            }
        }

        [Fact]
        public void Any_like()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities
                    .Where(e => new[] { "a%", "b%", "c%" }.Any(p => EF.Functions.Like(e.SomeText, p)))
                    .ToList();

                AssertSql(
                    @"SELECT s.""Id"", s.""SomeArray"", s.""SomeBytea"", s.""SomeList"", s.""SomeMatrix"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE s.""SomeText"" LIKE ANY (ARRAY['a%','b%','c%']::text[])");
            }
        }

        [Fact]
        public void Any_ilike()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities
                    .Where(e => new[] { "a%", "b%", "c%" }.Any(p => EF.Functions.ILike(e.SomeText, p)))
                    .ToList();

                AssertSql(
                    @"SELECT s.""Id"", s.""SomeArray"", s.""SomeBytea"", s.""SomeList"", s.""SomeMatrix"", s.""SomeText""
FROM ""SomeEntities"" AS s
WHERE s.""SomeText"" ILIKE ANY (ARRAY['a%','b%','c%']::text[])");
            }
        }

        [Fact]
        public void Any_like_anonymous()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var patterns = new[] { "a%", "b%", "c%" };

                var anon =
                    ctx.SomeEntities
                       .Select(
                           x => new
                           {
                               Array = x.SomeArray,
                               List = x.SomeList,
                               Text = x.SomeText
                           });

                var _ = anon.Where(x => patterns.Any(p => EF.Functions.Like(x.Text, p))).ToList();

                AssertSql(
                    @"@__patterns_0='System.String[]' (DbType = Object)

SELECT s.""SomeArray"" AS ""Array"", s.""SomeList"" AS ""List"", s.""SomeText"" AS ""Text""
FROM ""SomeEntities"" AS s
WHERE s.""SomeText"" LIKE ANY (@__patterns_0)");
            }
        }

        #endregion

        #region Support

        void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        void AssertDoesNotContainInSql(string expected)
            => Assert.DoesNotContain(expected, Fixture.TestSqlLoggerFactory.Sql);

        public class ArrayQueryContext : PoolableDbContext
        {
            public DbSet<SomeArrayEntity> SomeEntities { get; set; }

            public ArrayQueryContext(DbContextOptions options) : base(options) {}

            public static void Seed(ArrayQueryContext context)
            {
                context.SomeEntities.AddRange(
                    new SomeArrayEntity
                    {
                        Id = 1,
                        SomeArray = new[] { 3, 4 },
                        SomeBytea = new byte[] { 3, 4 },
                        SomeMatrix = new[,] { { 5, 6 }, { 7, 8 } },
                        SomeList = new List<int> { 3, 4 },
                        SomeText = "foo"
                    },
                    new SomeArrayEntity
                    {
                        Id = 2,
                        SomeArray = new[] { 5, 6, 7 },
                        SomeBytea = new byte[] { 5, 6, 7 },
                        SomeMatrix = new[,] { { 10, 11 }, { 12, 13 } },
                        SomeList = new List<int> { 3, 4 },
                        SomeText = "bar"
                    });
                context.SaveChanges();
            }
        }

        public class SomeArrayEntity
        {
            public int Id { get; set; }
            public int[] SomeArray { get; set; }
            public int[,] SomeMatrix { get; set; }
            public List<int> SomeList { get; set; }
            public byte[] SomeBytea { get; set; }
            public string SomeText { get; set; }
        }

        public class ArrayQueryFixture : SharedStoreFixtureBase<ArrayQueryContext>
        {
            protected override string StoreName => "ArrayQueryTest";
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
            protected override void Seed(ArrayQueryContext context) => ArrayQueryContext.Seed(context);
        }

        #endregion
    }
}
