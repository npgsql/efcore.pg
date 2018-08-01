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

        #region Roundtrip

        [Fact]
        public void Roundtrip()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.Id == 1);
                Assert.Equal(MappedEnum.Happy, x.MappedEnum);
            }
        }

        #endregion

        #region Where

        [Fact]
        public void Where_with_constant()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.MappedEnum == MappedEnum.Sad);
                Assert.Equal(MappedEnum.Sad, x.MappedEnum);
                AssertContainsInSql("WHERE e.\"MappedEnum\" = 'sad'::mapped_enum");
            }
        }

        [Fact]
        public void Where_with_constant_schema_qualified()
        {
            using (var ctx = Fixture.CreateContext())
            {
                var x = ctx.SomeEntities.Single(e => e.SchemaQualifiedEnum == SchemaQualifiedEnum.Happy);
                Assert.Equal(SchemaQualifiedEnum.Happy, x.SchemaQualifiedEnum);
                AssertContainsInSql("WHERE e.\"SchemaQualifiedEnum\" = 'happy'::test.schema_qualified_enum");
            }
        }

        [Fact]
        public void Where_with_parameter()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var sad = MappedEnum.Sad;
                var x = ctx.SomeEntities.Single(e => e.MappedEnum == sad);
                Assert.Equal(MappedEnum.Sad, x.MappedEnum);
                AssertContainsInSql("(DbType = Object)"); // Not very effective but better than nothing
                AssertContainsInSql("WHERE e.\"MappedEnum\" = @__sad_0");
            }
        }

        [Fact]
        public void Where_with_unmapped_enum_parameter_downcasts_are_implicit()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var sad = UnmappedEnum.Sad;
                var _ = ctx.SomeEntities.Single(e => e.UnmappedEnum == sad);
                AssertContainsInSql("@__sad_0='?' (DbType = Int32)");
                AssertContainsInSql("WHERE e.\"UnmappedEnum\" = @__sad_0");
            }
        }

        [Fact]
        public void Where_with_unmapped_enum_parameter_downcasts_do_not_matter()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var sad = UnmappedEnum.Sad;
                var _ = ctx.SomeEntities.Single(e => (int)e.UnmappedEnum == (int)sad);
                AssertContainsInSql("@__sad_0='?' (DbType = Int32)");
                AssertContainsInSql("WHERE e.\"UnmappedEnum\" = @__sad_0");
            }
        }

        [Fact]
        public void Where_with_mapped_enum_parameter_downcasts_do_not_matter()
        {
            using (var ctx = Fixture.CreateContext())
            {
                // ReSharper disable once ConvertToConstant.Local
                var sad = MappedEnum.Sad;
                var _ = ctx.SomeEntities.Single(e => (int)e.MappedEnum == (int)sad);
                AssertContainsInSql("@__sad_0='?' (DbType = Object)");
                AssertContainsInSql("WHERE e.\"MappedEnum\" = @__sad_0");
            }
        }

        #endregion

        #endregion

        #region Support

        EnumFixture Fixture { get; }

        public EnumQueryTest(EnumFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        void AssertContainsInSql(string expected)
            => Assert.Contains(expected, Fixture.TestSqlLoggerFactory.Sql);

        // ReSharper disable once UnusedMember.Local
        void AssertDoesNotContainInSql(string expected)
            => Assert.DoesNotContain(expected, Fixture.TestSqlLoggerFactory.Sql);

        public class EnumContext : DbContext
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Global
            public DbSet<SomeEnumEntity> SomeEntities { get; set; }

            static EnumContext()
            {
                NpgsqlConnection.GlobalTypeMapper.MapEnum<MappedEnum>();
                NpgsqlConnection.GlobalTypeMapper.MapEnum<InferredEnum>();
                NpgsqlConnection.GlobalTypeMapper.MapEnum<SchemaQualifiedEnum>("test.schema_qualified_enum");
            }

            public EnumContext(DbContextOptions options) : base(options) {}

            protected override void OnModelCreating(ModelBuilder builder)
                => builder.ForNpgsqlHasEnum("mapped_enum", new[] { "happy", "sad" })
                          .ForNpgsqlHasEnum<InferredEnum>()
                          .HasDefaultSchema("test")
                          .ForNpgsqlHasEnum<SchemaQualifiedEnum>();
        }

        public class SomeEnumEntity
        {
            public int Id { get; set; }

            public MappedEnum MappedEnum { get; set; }

            public UnmappedEnum UnmappedEnum { get; set; }

            public InferredEnum InferredEnum { get; set; }

            public SchemaQualifiedEnum SchemaQualifiedEnum { get; set; }

            public int EnumValue { get; set; }
        }

        public enum MappedEnum
        {
            Happy,
            Sad
        };

        public enum UnmappedEnum
        {
            Happy,
            Sad
        };

        public enum InferredEnum
        {
            Happy,
            Sad
        };

        public enum SchemaQualifiedEnum
        {
            Happy,
            Sad
        }

        // ReSharper disable once ClassNeverInstantiated.Global
        public class EnumFixture : IDisposable
        {
            readonly DbContextOptions _options;

            readonly NpgsqlTestStore _testStore;

            public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

            public EnumFixture()
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
                           new SomeEnumEntity
                           {
                               Id = 1,
                               MappedEnum = MappedEnum.Happy,
                               UnmappedEnum = UnmappedEnum.Happy,
                               InferredEnum = InferredEnum.Happy,
                               SchemaQualifiedEnum = SchemaQualifiedEnum.Happy,
                               EnumValue = (int)MappedEnum.Happy
                           },
                           new SomeEnumEntity
                           {
                               Id = 2,
                               MappedEnum = MappedEnum.Sad,
                               UnmappedEnum = UnmappedEnum.Sad,
                               InferredEnum = InferredEnum.Sad,
                               SchemaQualifiedEnum = SchemaQualifiedEnum.Sad,
                               EnumValue = (int)MappedEnum.Sad
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
