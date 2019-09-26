using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class CompatibilityQueryNpgsqlTest : IClassFixture<CompatibilityQueryNpgsqlTest.CompatibilityQueryNpgsqlFixture>
    {
        /// <summary>
        /// Provides resources for unit tests.
        /// </summary>
        CompatibilityQueryNpgsqlFixture Fixture { get; }

        /// <summary>
        /// Initializes resources for unit tests.
        /// </summary>
        /// <param name="fixture">The fixture of resources for testing.</param>
        public CompatibilityQueryNpgsqlTest(CompatibilityQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        // CURRENTLY EMPTY

        CompatibilityContext CreateContext(Version postgresVersion = null)
        {
            var builder = new DbContextOptionsBuilder(Fixture.CreateOptions());
            if (postgresVersion != null)
                new NpgsqlDbContextOptionsBuilder(builder).SetPostgresVersion(postgresVersion);
            return new CompatibilityContext(builder.Options);
        }

        #region Fixtures

        /// <summary>
        /// Represents a fixture suitable for testing backendVersion.
        /// </summary>
        public class CompatibilityQueryNpgsqlFixture : SharedStoreFixtureBase<CompatibilityContext>
        {
            protected override string StoreName => "CompatibilityTest";
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
            protected override void Seed(CompatibilityContext context) => CompatibilityContext.Seed(context);
        }

        /// <summary>
        /// Represents an entity suitable for testing backendVersion.
        /// </summary>
        public class CompatibilityTestEntity
        {
            /// <summary>
            /// The primary key.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// The date and time.
            /// </summary>
            public DateTime DateTime { get; set; }
        }

        /// <summary>
        /// Represents a database suitable for testing range operators.
        /// </summary>
        public class CompatibilityContext : PoolableDbContext
        {
            /// <summary>
            /// Represents a set of entities for backendVersion testing.
            /// </summary>
            public DbSet<CompatibilityTestEntity> CompatibilityTestEntities { get; set; }

            /// <inheritdoc />
            public CompatibilityContext(DbContextOptions options) : base(options) {}

            /// <inheritdoc />
            protected override void OnModelCreating(ModelBuilder builder) {}

            public static void Seed(CompatibilityContext context)
            {
                context.CompatibilityTestEntities.AddRange(
                    new CompatibilityTestEntity
                    {
                        Id = 1,
                        DateTime = new DateTime(2018, 06, 23)
                    },
                    new CompatibilityTestEntity
                    {
                        Id = 2,
                        DateTime = new DateTime(2018, 06, 23)
                    },
                    new CompatibilityTestEntity
                    {
                        Id = 3,
                        DateTime = new DateTime(2018, 06, 23)
                    });

                context.SaveChanges();
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Asserts that the SQL fragment appears in the logs.
        /// </summary>
        /// <param name="sql">The SQL statement or fragment to search for in the logs.</param>
        void AssertContainsSql(string sql) => Assert.Contains(sql, Fixture.TestSqlLoggerFactory.Sql);

        /// <summary>
        /// Asserts that the SQL fragment does not appears in the logs.
        /// </summary>
        /// <param name="sql">The SQL statement or fragment to search for in the logs.</param>
        void AssertDoesNotContainsSql(string sql) => Assert.DoesNotContain(sql, Fixture.TestSqlLoggerFactory.Sql);

        #endregion
    }
}
