using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    [MinimumPostgresVersion(12, 0)]
    public class CollationQueryTest : IClassFixture<CollationQueryTest.CollationQueryFixture>
    {
        CollationQueryFixture Fixture { get; }

        public CollationQueryTest(CollationQueryFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [ConditionalFact]
        public void Property_configured_as_insensitive()
        {
            using var ctx = CreateContext();

            var id = ctx.SomeEntities
                .Where(b => b.CI == "sometext")
                .Select(b => b.Id)
                .Single();
            Assert.Equal(1, id);

            AssertSql(
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""CI"" = 'sometext'
LIMIT 2");
        }

        [ConditionalFact]
        public void Explicit_collation()
        {
            using var ctx = CreateContext();

            var id = ctx.SomeEntities
                .Where(b => EF.Functions.Collate(b.CS, "ci_coll") == "sometext")
                .Select(b => b.Id)
                .Single();
            Assert.Equal(1, id);

            AssertSql(
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""CS"" COLLATE ""ci_coll"" = 'sometext'
LIMIT 2");
        }

        [ConditionalFact]
        public void String_Equals_insensitive()
        {
            using var ctx = CreateContext();

            var id = ctx.SomeEntities
                .Where(b => b.CSSupportsCI.Equals("sometext", StringComparison.OrdinalIgnoreCase))
                .Select(b => b.Id)
                .Single();

            Assert.Equal(1, id);

            AssertSql(
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""CSSupportsCI"" COLLATE ""ci_coll"" = 'sometext'
LIMIT 2");
        }

        [ConditionalFact]
        public void String_Equals_insensitive_throws_when_ci_collation_not_configured()
        {
            using var ctx = CreateContext();

            Assert.Throws<InvalidOperationException>(() =>
                ctx.SomeEntities
                    .Single(b => b.CS.Equals("sometext", StringComparison.OrdinalIgnoreCase)));
        }

        [ConditionalFact]
        public void String_Equals_insensitive_deep()
        {
            using var ctx = CreateContext();

            var id = ctx.SomeEntities
                .Where(b => b.CSSupportsCI
                    .Substring(0, b.CSSupportsCI.Length)
                    .Equals("sometext", StringComparison.OrdinalIgnoreCase))
                .Select(b => b.Id)
                .Single();

            Assert.Equal(1, id);

            AssertSql(
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE SUBSTRING(s.""CSSupportsCI"", 1, LENGTH(s.""CSSupportsCI"")::INT) COLLATE ""ci_coll"" = 'sometext'
LIMIT 2");
        }

        [ConditionalFact]
        public void Case_sensitive_collation_is_inherited()
        {
            using var ctx = CreateContext();

            var result = ctx.SomeEntities
                .Where(b => b.CI.Equals("sometext", StringComparison.Ordinal))
                .Select(b => b.Id)
                .ToList();

            Assert.Empty(result);

            AssertSql(
                @"SELECT s.""Id""
FROM ""SomeEntities"" AS s
WHERE s.""CI"" COLLATE ""POSIX"" = 'sometext'");
        }

        protected CollationQueryContext CreateContext() => Fixture.CreateContext();

        void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        public class CollationQueryContext : PoolableDbContext
        {
            public DbSet<CollationEntity> SomeEntities { get; set; }

            public CollationQueryContext(DbContextOptions options) : base(options) {}

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.HasCollation("ci_coll", "und-u-ks-level2", "icu", deterministic: false);

                // Define a collation for explicitly case-sensitive operations for all columns
                modelBuilder.UseCaseSensitiveCollation("POSIX");

                modelBuilder.Entity<CollationEntity>(e =>
                {
                    e.Property(ee => ee.CI).UseCollation("ci_coll");
                    e.Property(ee => ee.CSSupportsCI).UseCaseInsensitiveCollation("ci_coll");
                });
            }

            public static void Seed(CollationQueryContext context)
            {
                context.SomeEntities.AddRange(
                    new CollationEntity
                    {
                        Id = 1,
                        CS = "SomeText",
                        CI = "SomeText",
                        CSSupportsCI = "SomeText"
                    },
                    new CollationEntity
                    {
                        Id = 2,
                        CS = "AnotherText",
                        CI = "AnotherText",
                        CSSupportsCI = "AnotherText"
                    });
                context.SaveChanges();
            }
        }

        public class CollationEntity
        {
            public int Id { get; set; }

            /// <summary>
            /// Inherits the database default (case-sensitive), no configuration for explicitly case-sensitive
            /// or insensitive operations.
            /// </summary>
            public string CS { get; set; }

            /// <summary>
            /// Configured as case-insensitive, overriding the database default (case-sensitive)
            /// </summary>
            public string CI { get; set; }

            /// <summary>
            /// Inherits the database default (case-sensitive), also configured for explicitly case-insensitive operations.
            /// </summary>
            public string CSSupportsCI { get; set; }
        }

        public class CollationQueryFixture : SharedStoreFixtureBase<CollationQueryContext>
        {
            protected override string StoreName => "CollationQueryTest";
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
            protected override void Seed(CollationQueryContext context) => CollationQueryContext.Seed(context);
        }
    }
}
