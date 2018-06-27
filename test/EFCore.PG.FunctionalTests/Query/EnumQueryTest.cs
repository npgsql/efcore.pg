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
    public class EnumQueryTest : IClassFixture<EnumQueryTest.EnumFixture>
    {
        #region Tests

        [Fact]
        public void Roundtrip()
        {
            using (var ctx = CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.Id == 1);
                Assert.Equal(EnumType1.Sad, x.Enum1);
            }
        }

        [Fact]
        public void Where_with_constant()
        {
            using (var ctx = CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.Enum1 == EnumType1.Sad);
                Assert.Equal(EnumType1.Sad, x.Enum1);
                AssertContainsInSql("WHERE e.\"Enum1\" = 'sad'::enum_type1");
            }
        }

        [Fact]
        public void Where_with_parameter()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var sad = EnumType1.Sad;
                var x = ctx.SomeEntities.Single(e => e.Enum1 == sad);
                Assert.Equal(EnumType1.Sad, x.Enum1);
                AssertContainsInSql("(DbType = Object)"); // Not very effective but better than nothing
                AssertContainsInSql("WHERE e.\"Enum1\" = @__sad_0");
            }
        }

        [Fact]
        public void Where_with_parameter_downcast()
        {
            using (var ctx = CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var sad = EnumType1.Sad;
                var x = ctx.SomeEntities.Single(e => e.EnumValue == (int)sad);
                Assert.Equal((int)EnumType1.Sad, x.EnumValue);
                AssertContainsInSql("(DbType = Object)"); // Not very effective but better than nothing
                AssertContainsInSql("WHERE e.\"EnumValue\" = CAST(@__sad_0 AS integer)");
            }
        }

        #endregion

        #region Support

        EnumFixture Fixture { get; }

        public EnumQueryTest(EnumFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        EnumContext CreateContext() => Fixture.CreateContext();

        void AssertContainsInSql(string expected)
            => Assert.Contains(expected, Fixture.TestSqlLoggerFactory.Sql);

        // ReSharper disable once UnusedMember.Local
        void AssertDoesNotContainInSql(string expected)
            => Assert.DoesNotContain(expected, Fixture.TestSqlLoggerFactory.Sql);

        public class EnumContext : DbContext
        {
            public DbSet<SomeEnumEntity> SomeEntities { get; set; }
            public EnumContext(DbContextOptions options) : base(options) {}

            protected override void OnModelCreating(ModelBuilder builder)
            {
                builder.ForNpgsqlHasEnum("enum_type1", new[] { "happy", "sad" });
            }
        }

        public class SomeEnumEntity
        {
            public int Id { get; set; }

            public EnumType1 Enum1 { get; set; }

            public int EnumValue { get; set; }
        }

        public enum EnumType1
        {
            // ReSharper disable once UnusedMember.Global
            Happy,
            Sad
        };

        public class EnumFixture : IDisposable
        {
            readonly DbContextOptions _options;

            readonly NpgsqlTestStore _testStore;

            public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

            public EnumFixture()
            {
                NpgsqlConnection.GlobalTypeMapper.MapEnum<EnumType1>();

                _testStore = NpgsqlTestStore.CreateScratch();
                _options = new DbContextOptionsBuilder()
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
                       .Add(
                           new SomeEnumEntity
                           {
                               Id = 1,
                               Enum1 = EnumType1.Sad,
                               EnumValue = (int)EnumType1.Sad
                           });

                    ctx.SaveChanges();
                }
            }

            public EnumContext CreateContext() => new EnumContext(_options);

            public void Dispose() => _testStore.Dispose();
        }

        #endregion
    }
}
