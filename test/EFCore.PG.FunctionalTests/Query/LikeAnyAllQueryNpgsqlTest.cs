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
    public class LikeAnyAllQueryNpgsqlTest : IClassFixture<LikeAnyAllQueryNpgsqlTest.LikeAnyQueryNpgsqlFixture>
    {
        #region Setup

        /// <summary>
        /// Provides resources for unit tests.
        /// </summary>
        LikeAnyQueryNpgsqlFixture Fixture { get; }

        /// <summary>
        /// Initializes resources for unit tests.
        /// </summary>
        /// <param name="fixture">The fixture of resources for testing.</param>
        public LikeAnyAllQueryNpgsqlTest(LikeAnyQueryNpgsqlFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        #endregion

        #region LikeTests

        [Fact]
        public void Array_Any_Like()
        {
            using (LikeAnyContext context = Fixture.CreateContext())
            {
                var collection = new string[] { "a", "b", "c" };

                LikeAnyTestEntity[] _ =
                    context.LikeAnyTestEntities
                           .Where(x => collection.Any(y => EF.Functions.Like(x.Animal, y)))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Animal\" LIKE ANY (@__collection_0) = TRUE");
            }
        }

        [Fact]
        public void Array_All_Like()
        {
            using (LikeAnyContext context = Fixture.CreateContext())
            {
                var collection = new string[] { "a", "b", "c" };

                LikeAnyTestEntity[] _ =
                    context.LikeAnyTestEntities
                           .Where(x => collection.All(y => EF.Functions.Like(x.Animal, y)))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Animal\" LIKE ALL (@__collection_0) = TRUE");
            }
        }

        #endregion

        #region ILikeTests

        [Fact]
        public void Array_Any_ILike()
        {
            using (LikeAnyContext context = Fixture.CreateContext())
            {
                var collection = new string[] { "a", "b", "c%" };

                LikeAnyTestEntity[] _ =
                    context.LikeAnyTestEntities
                           .Where(x => collection.Any(y => EF.Functions.ILike(x.Animal, y)))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Animal\" ILIKE ANY (@__collection_0) = TRUE");
            }
        }

        [Fact]
        public void Array_All_ILike()
        {
            using (LikeAnyContext context = Fixture.CreateContext())
            {
                var collection = new string[] { "a", "b", "c%" };

                LikeAnyTestEntity[] _ =
                    context.LikeAnyTestEntities
                           .Where(x => collection.All(y => EF.Functions.ILike(x.Animal, y)))
                           .ToArray();

                AssertContainsSql("WHERE x.\"Animal\" ILIKE ALL (@__collection_0) = TRUE");
            }
        }

        #endregion

        #region Fixtures

        /// <summary>
        /// Represents a fixture suitable for testing LIKE ANY expressions.
        /// </summary>
        public class LikeAnyQueryNpgsqlFixture : IDisposable
        {
            /// <summary>
            /// The <see cref="NpgsqlTestStore"/> used for testing.
            /// </summary>
            readonly NpgsqlTestStore _testStore;

            /// <summary>
            /// The <see cref="DbContextOptions"/> used for testing.
            /// </summary>
            readonly DbContextOptions _options;

            /// <summary>
            /// The logger factory used for testing.
            /// </summary>
            public TestSqlLoggerFactory TestSqlLoggerFactory { get; }

            /// <summary>
            /// Initializes a <see cref="LikeAnyQueryNpgsqlFixture"/>.
            /// </summary>
            public LikeAnyQueryNpgsqlFixture()
            {
                TestSqlLoggerFactory = new TestSqlLoggerFactory();

                _testStore = NpgsqlTestStore.CreateScratch();

                _options =
                    new DbContextOptionsBuilder()
                        .UseNpgsql(_testStore.ConnectionString, b => b.ApplyConfiguration())
                        .UseInternalServiceProvider(
                            new ServiceCollection()
                                .AddEntityFrameworkNpgsql()
                                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                                .BuildServiceProvider())
                        .Options;

                using (LikeAnyContext context = CreateContext())
                {
                    context.Database.EnsureCreated();

                    context.LikeAnyTestEntities
                           .AddRange(
                               new LikeAnyTestEntity
                               {
                                   Id = 1,
                                   Animal = "cat"
                               },
                               new LikeAnyTestEntity
                               {
                                   Id = 2,
                                   Animal = "dog"
                               },
                               new LikeAnyTestEntity
                               {
                                   Id = 3,
                                   Animal = "turtle"
                               },
                               new LikeAnyTestEntity
                               {
                                   Id = 4,
                                   Animal = "bird"
                               });

                    context.SaveChanges();
                }
            }

            /// <summary>
            /// Creates a new <see cref="LikeAnyContext"/>.
            /// </summary>
            /// <returns>
            /// A <see cref="LikeAnyContext"/> for testing.
            /// </returns>
            public LikeAnyContext CreateContext() => new LikeAnyContext(_options);

            /// <inheritdoc />
            public void Dispose() => _testStore.Dispose();
        }

        /// <summary>
        /// Represents an entity suitable for testing LIKE ANY expressions.
        /// </summary>
        public class LikeAnyTestEntity
        {
            /// <summary>
            /// The primary key.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// The value.
            /// </summary>
            public string Animal { get; set; }
        }

        /// <summary>
        /// Represents a database suitable for testing range operators.
        /// </summary>
        public class LikeAnyContext : DbContext
        {
            /// <summary>
            /// Represents a set of entities with a string property.
            /// </summary>
            public DbSet<LikeAnyTestEntity> LikeAnyTestEntities { get; set; }

            /// <summary>
            /// Initializes a <see cref="LikeAnyContext"/>.
            /// </summary>
            /// <param name="options">
            /// The options to be used for configuration.
            /// </param>
            public LikeAnyContext(DbContextOptions options) : base(options) { }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Asserts that the SQL fragment appears in the logs.
        /// </summary>
        /// <param name="sql">The SQL statement or fragment to search for in the logs.</param>
        public void AssertContainsSql(string sql) => Assert.Contains(sql, Fixture.TestSqlLoggerFactory.Sql);

        #endregion
    }
}
