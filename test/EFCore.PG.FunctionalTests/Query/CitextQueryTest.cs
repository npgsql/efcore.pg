using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    /// <summary>
    /// Tests operations on the PostgreSQL citext type.
    /// </summary>
    public class CitextQueryTest : IClassFixture<CitextQueryTest.CitextQueryFixture>
    {
        private CitextQueryFixture Fixture { get; }

        // ReSharper disable once UnusedParameter.Local
        public CitextQueryTest(CitextQueryFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [Fact]
        public void StartsWith_literal()
        {
            using var ctx = CreateContext();
            var result = ctx.SomeEntities.Single(s => s.CaseInsensitiveText.StartsWith("some"));

            Assert.Equal(1, result.Id);
            AssertSql(
                @"SELECT s.""Id"", s.""CaseInsensitiveText""
FROM ""SomeEntities"" AS s
WHERE (s.""CaseInsensitiveText"" IS NOT NULL) AND (s.""CaseInsensitiveText"" LIKE 'some%')
LIMIT 2");
        }

        [Fact]
        public void StartsWith_param_pattern()
        {
            using var ctx = CreateContext();
            var param = "some";
            var result = ctx.SomeEntities.Single(s => s.CaseInsensitiveText.StartsWith(param));

            Assert.Equal(1, result.Id);
            AssertSql(
                @"@__param_0='some'

SELECT s.""Id"", s.""CaseInsensitiveText""
FROM ""SomeEntities"" AS s
WHERE (@__param_0 = '') OR ((s.""CaseInsensitiveText"" IS NOT NULL) AND ((s.""CaseInsensitiveText"" LIKE @__param_0 || '%' ESCAPE '') AND (left(s.""CaseInsensitiveText"", length(@__param_0))::citext = @__param_0::citext)))
LIMIT 2");
        }

        [Fact]
        public void StartsWith_param_instance()
        {
            using var ctx = CreateContext();
            var param = "SomeTextWithExtraStuff";
            var result = ctx.SomeEntities.Single(s => param.StartsWith(s.CaseInsensitiveText));

            Assert.Equal(1, result.Id);
            AssertSql(
                @"@__param_0='SomeTextWithExtraStuff'

SELECT s.""Id"", s.""CaseInsensitiveText""
FROM ""SomeEntities"" AS s
WHERE (s.""CaseInsensitiveText"" = '') OR ((s.""CaseInsensitiveText"" IS NOT NULL) AND ((@__param_0 LIKE s.""CaseInsensitiveText"" || '%' ESCAPE '') AND (left(@__param_0, length(s.""CaseInsensitiveText""))::citext = s.""CaseInsensitiveText""::citext)))
LIMIT 2");
        }

        [Fact]
        public void EndsWith_literal()
        {
            using var ctx = CreateContext();
            var result = ctx.SomeEntities.Single(s => s.CaseInsensitiveText.EndsWith("sometext"));

            Assert.Equal(1, result.Id);
            AssertSql(
                @"SELECT s.""Id"", s.""CaseInsensitiveText""
FROM ""SomeEntities"" AS s
WHERE (s.""CaseInsensitiveText"" IS NOT NULL) AND (s.""CaseInsensitiveText"" LIKE '%sometext')
LIMIT 2");
        }

        [Fact]
        public void EndsWith_param_pattern()
        {
            using var ctx = CreateContext();
            var param = "sometext";
            var result = ctx.SomeEntities.Single(s => s.CaseInsensitiveText.EndsWith(param));

            Assert.Equal(1, result.Id);
            AssertSql(
                @"@__param_0='sometext'

SELECT s.""Id"", s.""CaseInsensitiveText""
FROM ""SomeEntities"" AS s
WHERE (@__param_0 = '') OR ((s.""CaseInsensitiveText"" IS NOT NULL) AND (right(s.""CaseInsensitiveText"", length(@__param_0))::citext = @__param_0::citext))
LIMIT 2");
        }

        [Fact]
        public void EndsWith_param_instance()
        {
            using var ctx = CreateContext();
            var param = "ExtraStuffThenSomeText";
            var result = ctx.SomeEntities.Single(s => param.EndsWith(s.CaseInsensitiveText));

            Assert.Equal(1, result.Id);
            AssertSql(
                @"@__param_0='ExtraStuffThenSomeText'

SELECT s.""Id"", s.""CaseInsensitiveText""
FROM ""SomeEntities"" AS s
WHERE (s.""CaseInsensitiveText"" = '') OR ((s.""CaseInsensitiveText"" IS NOT NULL) AND (right(@__param_0, length(s.""CaseInsensitiveText""))::citext = s.""CaseInsensitiveText""::citext))
LIMIT 2");
        }

        [Fact]
        public void Contains_literal()
        {
            using var ctx = CreateContext();
            var result = ctx.SomeEntities.Single(s => s.CaseInsensitiveText.Contains("ometex"));

            Assert.Equal(1, result.Id);
            AssertSql(
                @"SELECT s.""Id"", s.""CaseInsensitiveText""
FROM ""SomeEntities"" AS s
WHERE strpos(s.""CaseInsensitiveText"", 'ometex') > 0
LIMIT 2");
        }

        [Fact]
        public void Contains_param_pattern()
        {
            using var ctx = CreateContext();
            var param = "ometex";
            var result = ctx.SomeEntities.Single(s => s.CaseInsensitiveText.Contains(param));

            Assert.Equal(1, result.Id);
            AssertSql(
                @"@__param_0='ometex'

SELECT s.""Id"", s.""CaseInsensitiveText""
FROM ""SomeEntities"" AS s
WHERE (@__param_0 = '') OR (strpos(s.""CaseInsensitiveText"", @__param_0) > 0)
LIMIT 2");
        }

        [Fact]
        public void Contains_param_instance()
        {
            using var ctx = CreateContext();
            var param = "ExtraSometextExtra";
            var result = ctx.SomeEntities.Single(s => param.Contains(s.CaseInsensitiveText));

            Assert.Equal(1, result.Id);
            AssertSql(
                @"@__param_0='ExtraSometextExtra'

SELECT s.""Id"", s.""CaseInsensitiveText""
FROM ""SomeEntities"" AS s
WHERE (s.""CaseInsensitiveText"" = '') OR (strpos(@__param_0, s.""CaseInsensitiveText"") > 0)
LIMIT 2");
        }

        [Fact]
        public void IndexOf_literal()
        {
            using var ctx = CreateContext();
            var result = ctx.SomeEntities.Single(s => s.CaseInsensitiveText.IndexOf("ometex") == 1);

            Assert.Equal(1, result.Id);
            AssertSql(
                @"SELECT s.""Id"", s.""CaseInsensitiveText""
FROM ""SomeEntities"" AS s
WHERE (strpos(s.""CaseInsensitiveText"", 'ometex') - 1) = 1
LIMIT 2");
        }

        [Fact]
        public void IndexOf_param_pattern()
        {
            using var ctx = CreateContext();
            var param = "ometex";
            var result = ctx.SomeEntities.Single(s => s.CaseInsensitiveText.IndexOf(param) == 1);

            Assert.Equal(1, result.Id);
            AssertSql(
                @"@__param_0='ometex'

SELECT s.""Id"", s.""CaseInsensitiveText""
FROM ""SomeEntities"" AS s
WHERE (strpos(s.""CaseInsensitiveText"", @__param_0) - 1) = 1
LIMIT 2");
        }

        [Fact]
        public void IndexOf_param_instance()
        {
            using var ctx = CreateContext();
            var param = "ExtraSometextExtra";
            var result = ctx.SomeEntities.Single(s => param.IndexOf(s.CaseInsensitiveText) == 5);

            Assert.Equal(1, result.Id);
            AssertSql(
                @"@__param_0='ExtraSometextExtra'

SELECT s.""Id"", s.""CaseInsensitiveText""
FROM ""SomeEntities"" AS s
WHERE (strpos(@__param_0, s.""CaseInsensitiveText"") - 1) = 5
LIMIT 2");
        }

        [Fact]
        public void Replace_literal()
        {
            using var ctx = CreateContext();
            var result = ctx.SomeEntities.Single(s => s.CaseInsensitiveText.Replace("Te", "Ne") == "SomeNext");

            Assert.Equal(1, result.Id);
            AssertSql(
                @"SELECT s.""Id"", s.""CaseInsensitiveText""
FROM ""SomeEntities"" AS s
WHERE replace(s.""CaseInsensitiveText"", 'Te', 'Ne') = 'SomeNext'
LIMIT 2");
        }

        [Fact]
        public void Replace_param_pattern()
        {
            using var ctx = CreateContext();
            var param = "Te";
            var result = ctx.SomeEntities.Single(s => s.CaseInsensitiveText.Replace(param, "Ne") == "SomeNext");

            Assert.Equal(1, result.Id);
            AssertSql(
                @"@__param_0='Te'

SELECT s.""Id"", s.""CaseInsensitiveText""
FROM ""SomeEntities"" AS s
WHERE replace(s.""CaseInsensitiveText"", @__param_0, 'Ne') = 'SomeNext'
LIMIT 2");
        }

        [Fact]
        public void Replace_param_instance()
        {
            using var ctx = CreateContext();
            var param = "ExtraSometextExtra";
            var result = ctx.SomeEntities.Single(s => param.Replace(s.CaseInsensitiveText, "NewStuff") == "ExtraNewStuffExtra");

            Assert.Equal(1, result.Id);
            AssertSql(
                @"@__param_0='ExtraSometextExtra'

SELECT s.""Id"", s.""CaseInsensitiveText""
FROM ""SomeEntities"" AS s
WHERE replace(@__param_0, s.""CaseInsensitiveText"", 'NewStuff') = 'ExtraNewStuffExtra'
LIMIT 2");
        }

        protected CitextQueryContext CreateContext() => Fixture.CreateContext();

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class CitextQueryContext : PoolableDbContext
        {
            public DbSet<SomeArrayEntity> SomeEntities { get; set; }

            public CitextQueryContext(DbContextOptions options) : base(options) {}

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.HasPostgresExtension("citext");

            public static void Seed(CitextQueryContext context)
            {
                context.SomeEntities.AddRange(
                    new SomeArrayEntity { Id = 1, CaseInsensitiveText = "SomeText" },
                    new SomeArrayEntity { Id = 2, CaseInsensitiveText = "AnotherText" });
                context.SaveChanges();
            }
        }

        public class SomeArrayEntity
        {
            public int Id { get; set; }
            [Column(TypeName = "citext")]
            public string CaseInsensitiveText { get; set; }
        }

        public class CitextQueryFixture : SharedStoreFixtureBase<CitextQueryContext>
        {
            protected override string StoreName => "CitextQueryTest";
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
            protected override void Seed(CitextQueryContext context) => CitextQueryContext.Seed(context);
        }
    }
}
