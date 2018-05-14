using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using NpgsqlTypes;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    /// <summary>
    /// Represents a fixture suitable for testing range operators.
    /// </summary>
    public class RangeQueryNpgsqlFixture : IDisposable
    {
        /// <summary>
        /// The <see cref="NpgsqlTestStore"/> used for testing.
        /// </summary>
        private readonly NpgsqlTestStore _testStore;

        /// <summary>
        /// The <see cref="DbContextOptions"/> used for testing.
        /// </summary>
        private readonly DbContextOptions _options;

        /// <summary>
        /// The logger factory used for testing.
        /// </summary>
        public TestSqlLoggerFactory TestSqlLoggerFactory { get; }

        /// <summary>
        /// Initializes a <see cref="RangeQueryNpgsqlFixture"/>.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public RangeQueryNpgsqlFixture()
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

            using (RangeContext context = CreateContext())
            {
                context.Database.EnsureCreated();

                context.RangeTestEntities.AddRange(
                    new RangeTestEntity
                    {
                        Id = 1,
                        // (0, 10)
                        Range = new NpgsqlRange<int>(0, false, false, 10, false, false),
                    },
                    new RangeTestEntity
                    {
                        Id = 2,
                        // [0, 10)
                        Range = new NpgsqlRange<int>(0, true, false, 10, false, false)
                    },
                    new RangeTestEntity
                    {
                        Id = 3,
                        // [0, 10]
                        Range = new NpgsqlRange<int>(0, true, false, 10, true, false)
                    },
                    new RangeTestEntity
                    {
                        Id = 4,
                        // [0, ∞)
                        Range = new NpgsqlRange<int>(0, true, false, 0, false, true)
                    },
                    new RangeTestEntity
                    {
                        Id = 5,
                        // (-∞, 10]
                        Range = new NpgsqlRange<int>(0, false, true, 10, true, false)
                    },
                    new RangeTestEntity
                    {
                        Id = 6,
                        // (-∞, ∞)
                        Range = new NpgsqlRange<int>(0, false, true, 0, false, true)
                    },
                    new RangeTestEntity
                    {
                        Id = 7,
                        // (-∞, ∞)
                        Range = new NpgsqlRange<int>(0, false, true, 0, false, true)
                    });

                context.SaveChanges();
            }
        }

        /// <summary>
        /// Creates a new <see cref="RangeContext"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="RangeContext"/> for testing.
        /// </returns>
        public RangeContext CreateContext()
        {
            return new RangeContext(_options);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _testStore.Dispose();
        }
    }

    /// <summary>
    /// Represents an entity suitable for testing range operators.
    /// </summary>
    public class RangeTestEntity
    {
        /// <summary>
        /// The primary key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The range of integers.
        /// </summary>
        public NpgsqlRange<int> Range { get; set; }
    }

    /// <summary>
    /// Represents a database suitable for testing range operators.
    /// </summary>
    public class RangeContext : DbContext
    {
        /// <summary>
        /// Represents a set of entities with <see cref="NpgsqlRange{T}"/> properties.
        /// </summary>
        public DbSet<RangeTestEntity> RangeTestEntities { get; set; }

        /// <summary>
        /// Initializes a <see cref="RangeContext"/>.
        /// </summary>
        /// <param name="options">
        /// The options to be used for configuration.
        /// </param>
        public RangeContext(DbContextOptions options) : base(options) { }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder builder) { }
    }
}
