using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    /// <summary>
    /// Provides unit tests for the unaccent module operator and function translations.
    /// </summary>
    /// <remarks>
    /// See: https://www.postgresql.org/docs/current/unaccent.html
    /// </remarks>
    public class UnaccentQueryNpgsqlTest : IClassFixture<UnaccentQueryNpgsqlTest.UnaccentQueryNpgsqlFixture>
    {
        UnaccentQueryNpgsqlFixture Fixture { get; }

        public UnaccentQueryNpgsqlTest(UnaccentQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        #region FunctionTests

        [Fact]
        public void Unaccent()
        {
            using var context = CreateContext();
            var _ = context.UnaccentTestEntities
                .Select(x => EF.Functions.Unaccent(x.Text))
                .ToArray();

            AssertContainsSql(@"unaccent(u.""Text"")");
        }

        [Fact]
        public void Unaccent_with_regdictionary()
        {
            using var context = CreateContext();
            var _ = context.UnaccentTestEntities
                .Select(x => EF.Functions.Unaccent("unaccent", x.Text))
                .ToArray();

            AssertContainsSql(@"unaccent('unaccent', u.""Text"")");
        }

        #endregion

        #region Fixtures

        /// <summary>
        /// Represents a fixture suitable for testing unaccent operators.
        /// </summary>
        public class UnaccentQueryNpgsqlFixture : SharedStoreFixtureBase<UnaccentContext>
        {
            protected override string StoreName => "UnaccentQueryTest";

            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
            protected override void Seed(UnaccentContext context) => UnaccentContext.Seed(context);
        }

        /// <summary>
        /// Represents an entity suitable for testing unaccent operators.
        /// </summary>
        public class UnaccentTestEntity
        {
            // ReSharper disable once UnusedMember.Global
            /// <summary>
            /// The primary key.
            /// </summary>
            [Key]
            public int Id { get; set; }

            /// <summary>
            /// Some text.
            /// </summary>
            public string Text { get; set; }
        }

        /// <summary>
        /// Represents a database suitable for testing unaccent operators.
        /// </summary>
        public class UnaccentContext : PoolableDbContext
        {
            /// <summary>
            /// Represents a set of entities with <see cref="string"/> properties.
            /// </summary>
            public DbSet<UnaccentTestEntity> UnaccentTestEntities { get; set; }

            /// <summary>
            /// Initializes a <see cref="UnaccentContext"/>.
            /// </summary>
            /// <param name="options">
            /// The options to be used for configuration.
            /// </param>
            public UnaccentContext(DbContextOptions options) : base(options) { }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.HasPostgresExtension("unaccent");

                base.OnModelCreating(modelBuilder);
            }

            public static void Seed(UnaccentContext context)
            {
                for (var i = 1; i <= 9; i++)
                {
                    var text = "Some text " + i;
                    context.UnaccentTestEntities.Add(
                       new UnaccentTestEntity
                       {
                           Id = i,
                           Text = text
                       });
                }
                context.SaveChanges();
            }
        }

        #endregion

        #region Helpers

        protected UnaccentContext CreateContext() => Fixture.CreateContext();

        void AssertSql(params string[] expected) => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        /// <summary>
        /// Asserts that the SQL fragment appears in the logs.
        /// </summary>
        /// <param name="sql">The SQL statement or fragment to search for in the logs.</param>
        void AssertContainsSql(string sql) => Assert.Contains(sql, Fixture.TestSqlLoggerFactory.Sql);

        #endregion
    }
}
