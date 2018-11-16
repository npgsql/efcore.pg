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
    public class GuidNpgsqlTest : IClassFixture<GuidNpgsqlTest.GuidFixture>
    {
        #region Tests

        [Fact]
        public void OrderBy_Guid_NewGuid()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.OrderBy(e => Guid.NewGuid()).ToArray();

                AssertContainsInSql("ORDER BY uuid_generate_v4()");
            }
        }

        [Fact]
        public void Select_Guid_NewGuid()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(e => new { e.Guid, NewGuid = Guid.NewGuid() }).ToArray();

                AssertContainsInSql("SELECT e.\"Guid\", uuid_generate_v4() AS \"NewGuid\"");
            }
        }

        [Fact]
        public void Where_Guid_NewGuid()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Where(e => e.Guid == Guid.NewGuid()).ToArray();

                AssertContainsInSql("WHERE e.\"Guid\" = uuid_generate_v4()");
            }
        }

        #endregion

        #region Support

        GuidFixture Fixture { get; }

        public GuidNpgsqlTest(GuidFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        void AssertContainsInSql(string expected)
            => Assert.Contains(expected, Fixture.TestSqlLoggerFactory.Sql);

        public class GuidContext : DbContext
        {
            public DbSet<SomeGuidEntity> SomeEntities { get; set; }

            public GuidContext(DbContextOptions options) : base(options) {}

            protected override void OnModelCreating(ModelBuilder builder)
                => builder.HasPostgresExtension("uuid-ossp");
        }

        public class SomeGuidEntity
        {
            public int Id { get; set; }
            public Guid Guid { get; set; }
        }

        public class GuidFixture : IDisposable
        {
            readonly DbContextOptions _options;

            readonly NpgsqlTestStore _testStore;

            public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

            public GuidFixture()
            {
                _testStore = NpgsqlTestStore.CreateScratch();

                _options =
                    new DbContextOptionsBuilder()
                        .UseNpgsql(_testStore.ConnectionString)
                        .UseInternalServiceProvider(
                            new ServiceCollection()
                                .AddEntityFrameworkNpgsql()
                                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                                .BuildServiceProvider())
                        .Options;

                using (var ctx = CreateContext())
                {
                    ctx.Database.EnsureCreated();
                    ctx.SomeEntities.Add(new SomeGuidEntity { Id = 1, Guid = Guid.NewGuid() });
                    ctx.SaveChanges();
                }
            }

            public GuidContext CreateContext() => new GuidContext(_options);

            public void Dispose() => _testStore.Dispose();
        }

        #endregion
    }
}
