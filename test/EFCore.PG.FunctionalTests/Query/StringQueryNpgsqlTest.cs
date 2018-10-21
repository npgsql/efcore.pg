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
    /// <summary>
    /// Provides unit tests for range operator translations.
    /// </summary>
    public class StringQueryNpgsqlTest : IClassFixture<StringQueryNpgsqlTest.StringQueryNpgsqlFixture>
    {
        #region Tests

        #region PadLeft, PadRight

        [Fact]
        public void PadLeft_with_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(x => x.SomeString.PadLeft(2)).ToArray();
                AssertContainsInSql("SELECT lpad(x.\"SomeString\", 2)");
            }
        }

        [Fact]
        public void PadLeft_char_with_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(x => x.SomeString.PadLeft(2, 'a')).ToArray();
                AssertContainsInSql("SELECT lpad(x.\"SomeString\", 2, 'a')");
            }
        }

        [Fact]
        public void PadLeft_with_parameter()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var length = 2;
                var _ = ctx.SomeEntities.Select(x => x.SomeString.PadLeft(length)).ToArray();
                AssertContainsInSql("SELECT lpad(x.\"SomeString\", @__length_0)");
            }
        }

        [Fact]
        public void PadLeft_char_with_parameter()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var length = 2;
                // ReSharper disable once ConvertToConstant.Local
                var character = 'a';
                var _ = ctx.SomeEntities.Select(x => x.SomeString.PadLeft(length, character)).ToArray();
                AssertContainsInSql("SELECT lpad(x.\"SomeString\", @__length_0, @__character_1)");
            }
        }

        [Fact]
        public void PadRight_with_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(x => x.SomeString.PadRight(2)).ToArray();
                AssertContainsInSql("SELECT rpad(x.\"SomeString\", 2)");
            }
        }

        [Fact]
        public void PadRight_char_with_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var _ = ctx.SomeEntities.Select(x => x.SomeString.PadRight(2, 'a')).ToArray();
                AssertContainsInSql("SELECT rpad(x.\"SomeString\", 2, 'a')");
            }
        }

        [Fact]
        public void PadRight_with_parameter()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var length = 2;
                var _ = ctx.SomeEntities.Select(x => x.SomeString.PadRight(length)).ToArray();
                AssertContainsInSql("SELECT rpad(x.\"SomeString\", @__length_0)");
            }
        }

        [Fact]
        public void PadRight_char_with_parameter()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var length = 2;
                // ReSharper disable once ConvertToConstant.Local
                var character = 'a';
                var _ = ctx.SomeEntities.Select(x => x.SomeString.PadRight(length, character)).ToArray();
                AssertContainsInSql("SELECT rpad(x.\"SomeString\", @__length_0, @__character_1)");
            }
        }

        #endregion

        #endregion

        #region Support

        StringQueryNpgsqlFixture Fixture { get; }

        public StringQueryNpgsqlTest(StringQueryNpgsqlFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        void AssertContainsInSql(string expected)
            => Assert.Contains(expected, Fixture.TestSqlLoggerFactory.Sql);

        // ReSharper disable once MemberCanBePrivate.Global
        public class StringContext : DbContext
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Global
            public DbSet<SomeStringEntity> SomeEntities { get; set; }

            public StringContext(DbContextOptions options) : base(options) {}
        }

        public class SomeStringEntity
        {
            public int Id { get; set; }

            public string SomeString { get; set; }
        }

        // ReSharper disable once ClassNeverInstantiated.Global
        public class StringQueryNpgsqlFixture : IDisposable
        {
            readonly DbContextOptions _options;

            readonly NpgsqlTestStore _testStore;

            public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

            public StringQueryNpgsqlFixture()
            {
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

                using (var ctx = CreateContext())
                {
                    ctx.Database.EnsureCreated();

                    ctx.SomeEntities
                       .AddRange(
                           new SomeStringEntity
                           {
                               Id = 1,
                               SomeString = ""
                           },
                           new SomeStringEntity
                           {
                               Id = 2,
                               SomeString = "SomeString"
                           });

                    ctx.SaveChanges();
                }
            }

            public StringContext CreateContext() => new StringContext(_options);

            public void Dispose() => _testStore.Dispose();
        }

        #endregion
    }
}
