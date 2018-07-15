using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class CompatibilityQueryNpgsqlTest : IClassFixture<CompatibilityQueryNpgsqlTest.CompatibilityQueryNpgsqlFixure>
    {
        /// <summary>
        /// Provides resources for unit tests.
        /// </summary>
        CompatibilityQueryNpgsqlFixure Fixture { get; }

        /// <summary>
        /// Initializes resources for unit tests.
        /// </summary>
        /// <param name="fixture">The fixture of resources for testing.</param>
        public CompatibilityQueryNpgsqlTest(CompatibilityQueryNpgsqlFixure fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        #region Tests

        [Theory]
        [InlineData("9.4")]
        [InlineData("9.5")]
        [InlineData("9.6")]
        [InlineData("10.0")]
        [InlineData("10.1")]
        [InlineData("10.2")]
        [InlineData("10.3")]
        [InlineData("10.4")]
        public void GivenDateTimeAdd_WhenVersionIsSupported_ThenTranslates(string version)
        {
            using (CompatibilityContext context = Fixture.CreateContext(postgresVersion: Version.Parse(version)))
            {
                // ReSharper disable once ConvertToConstant.Local
                int years = 2;

                DateTime[] _ =
                    context.CompatibilityTestEntities
                           .Select(x => x.DateTime.AddYears(years))
                           .ToArray();

                AssertContainsSql("SELECT (x.\"DateTime\" + MAKE_INTERVAL(years => @__years_0))");
            }
        }

        [Theory]
        [InlineData("9.0")]
        [InlineData("9.1")]
        [InlineData("9.2")]
        [InlineData("9.3")]
        public void GivenDateTimeAdd_WhenVersionIsNotSupported_ThenDoesNotTranslate(string version)
        {
            using (CompatibilityContext context = Fixture.CreateContext(postgresVersion: Version.Parse(version)))
            {
                // ReSharper disable once ConvertToConstant.Local
                int years = 2;

                DateTime[] _ =
                    context.CompatibilityTestEntities
                           .Select(x => x.DateTime.AddYears(years))
                           .ToArray();

                AssertDoesNotContainsSql("SELECT (x.\"DateTime\" + MAKE_INTERVAL(years => @__years_0))");
            }
        }

        #endregion

        #region Fixtures

        /// <summary>
        /// Represents a fixture suitable for testing backendVersion.
        /// </summary>
        public class CompatibilityQueryNpgsqlFixure : IDisposable
        {
            /// <summary>
            /// The <see cref="NpgsqlTestStore"/> used for testing.
            /// </summary>
            private readonly NpgsqlTestStore _testStore;

            /// <summary>
            /// The logger factory used for testing.
            /// </summary>
            public TestSqlLoggerFactory TestSqlLoggerFactory { get; }

            /// <summary>
            /// Initializes a <see cref="CompatibilityQueryNpgsqlFixure"/>.
            /// </summary>
            // ReSharper disable once UnusedMember.Global
            public CompatibilityQueryNpgsqlFixure()
            {
                TestSqlLoggerFactory = new TestSqlLoggerFactory();

                _testStore = NpgsqlTestStore.CreateScratch();

                using (CompatibilityContext context = CreateContext())
                {
                    context.Database.EnsureCreated();

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

            /// <inheritdoc />
            public void Dispose() => _testStore.Dispose();

            /// <summary>
            /// Creates a new <see cref="CompatibilityContext"/>.
            /// </summary>
            /// <param name="postgresVersion">The backend version to target.</param>
            /// <returns>
            /// A <see cref="CompatibilityContext"/> for testing.
            /// </returns>
            public CompatibilityContext CreateContext(Version postgresVersion = null)
                => new CompatibilityContext(
                    new DbContextOptionsBuilder()
                        .UseNpgsql(
                            _testStore.ConnectionString,
                            x =>
                            {
                                x.ApplyConfiguration();
                                if (postgresVersion != null)
                                    x.SetPostgresVersion(postgresVersion);
                            })
                        .UseInternalServiceProvider(
                            new ServiceCollection()
                                .AddEntityFrameworkNpgsql()
                                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                                .BuildServiceProvider())
                        .Options);
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
        public class CompatibilityContext : DbContext
        {
            /// <summary>
            /// Represents a set of entities for backendVersion testing.
            /// </summary>
            public DbSet<CompatibilityTestEntity> CompatibilityTestEntities { get; set; }

            /// <inheritdoc />
            public CompatibilityContext(DbContextOptions options) : base(options) {}

            /// <inheritdoc />
            protected override void OnModelCreating(ModelBuilder builder) {}
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Asserts that the SQL fragment appears in the logs.
        /// </summary>
        /// <param name="sql">The SQL statement or fragment to search for in the logs.</param>
        public void AssertContainsSql(string sql)
        {
            Assert.Contains(sql, Fixture.TestSqlLoggerFactory.Sql);
        }

        /// <summary>
        /// Asserts that the SQL fragment does not appears in the logs.
        /// </summary>
        /// <param name="sql">The SQL statement or fragment to search for in the logs.</param>
        public void AssertDoesNotContainsSql(string sql)
        {
            Assert.DoesNotContain(sql, Fixture.TestSqlLoggerFactory.Sql);
        }

        #endregion
    }
}
