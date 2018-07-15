using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class EnumQueryTest : IClassFixture<EnumFixture>
    {
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
                AssertContainsInSql("WHERE e.enum1 = 'sad'::enum_type1");
            }
        }

        [Fact]
        public void Where_with_parameter()
        {
            using (var ctx = CreateContext())
            {
                var sad = EnumType1.Sad;
                var x = ctx.SomeEntities.Single(e => e.Enum1 == sad);
                Assert.Equal(EnumType1.Sad, x.Enum1);
                AssertContainsInSql("(DbType = Object)");  // Not very effective but better than nothing
                AssertContainsInSql("WHERE e.enum1 = @");
            }
        }

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

        void AssertDoesNotContainInSql(string expected)
            => Assert.DoesNotContain(expected, Fixture.TestSqlLoggerFactory.Sql);

        #endregion Support
    }

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
        [Column("enum1")]
        public EnumType1 Enum1 { get; set; }
    }

    public enum EnumType1 { Happy, Sad };

    public class EnumFixture : IDisposable
    {
        readonly DbContextOptions _options;
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
                ctx.SomeEntities.Add(new SomeEnumEntity
                {
                    Id=1,
                    Enum1 = EnumType1.Sad
                });
                ctx.SaveChanges();
            }
        }

        readonly NpgsqlTestStore _testStore;
        public EnumContext CreateContext() => new EnumContext(_options);
        public void Dispose() => _testStore.Dispose();
    }
}
