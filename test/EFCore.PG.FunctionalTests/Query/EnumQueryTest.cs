using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using NpgsqlTypes;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class EnumQueryTest : IClassFixture<EnumQueryTest.EnumFixture>
    {
        EnumFixture Fixture { get; }

        public EnumQueryTest(EnumFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        #region Roundtrip

        [Fact]
        public void Roundtrip()
        {
            using var ctx = CreateContext();
            var x = ctx.SomeEntities.Single(e => e.Id == 1);
            Assert.Equal(MappedEnum.Happy, x.MappedEnum);
        }

        #endregion

        #region Where

        [Fact]
        public void Where_with_constant()
        {
            using var ctx = CreateContext();
            var x = ctx.SomeEntities.Single(e => e.MappedEnum == MappedEnum.Sad);
            Assert.Equal(MappedEnum.Sad, x.MappedEnum);

            AssertContainsInSql(@"WHERE s.""MappedEnum"" = 'sad'::test.mapped_enum");
        }

        [Fact]
        public void Where_with_constant_schema_qualified()
        {
            using var ctx = CreateContext();
            var x = ctx.SomeEntities.Single(e => e.SchemaQualifiedEnum == SchemaQualifiedEnum.Happy);
            Assert.Equal(SchemaQualifiedEnum.Happy, x.SchemaQualifiedEnum);

            AssertContainsInSql(@"WHERE s.""SchemaQualifiedEnum"" = 'Happy (PgName)'::test.schema_qualified_enum");
        }

        [Fact]
        public void Where_with_parameter()
        {
            using var ctx = CreateContext();
            // ReSharper disable once ConvertToConstant.Local
            var sad = MappedEnum.Sad;
            var x = ctx.SomeEntities.Single(e => e.MappedEnum == sad);
            Assert.Equal(MappedEnum.Sad, x.MappedEnum);

            AssertSql(
                @"@__sad_0='Sad' (DbType = Object)

SELECT s.""Id"", s.""ByteEnum"", s.""EnumValue"", s.""InferredEnum"", s.""MappedEnum"", s.""SchemaQualifiedEnum"", s.""UnmappedByteEnum"", s.""UnmappedEnum""
FROM test.""SomeEntities"" AS s
WHERE s.""MappedEnum"" = @__sad_0
LIMIT 2");
        }

        [Fact]
        public void Where_with_unmapped_enum_parameter_downcasts_are_implicit()
        {
            using var ctx = CreateContext();
            // ReSharper disable once ConvertToConstant.Local
            var sad = UnmappedEnum.Sad;
            var _ = ctx.SomeEntities.Single(e => e.UnmappedEnum == sad);

            AssertSql(
                @"@__sad_0='1'

SELECT s.""Id"", s.""ByteEnum"", s.""EnumValue"", s.""InferredEnum"", s.""MappedEnum"", s.""SchemaQualifiedEnum"", s.""UnmappedByteEnum"", s.""UnmappedEnum""
FROM test.""SomeEntities"" AS s
WHERE s.""UnmappedEnum"" = @__sad_0
LIMIT 2");
        }

        [Fact]
        public void Where_with_unmapped_enum_parameter_downcasts_do_not_matter()
        {
            using var ctx = CreateContext();
            // ReSharper disable once ConvertToConstant.Local
            var sad = UnmappedEnum.Sad;
            var _ = ctx.SomeEntities.Single(e => (int)e.UnmappedEnum == (int)sad);

            AssertSql(
                @"@__sad_0='1'

SELECT s.""Id"", s.""ByteEnum"", s.""EnumValue"", s.""InferredEnum"", s.""MappedEnum"", s.""SchemaQualifiedEnum"", s.""UnmappedByteEnum"", s.""UnmappedEnum""
FROM test.""SomeEntities"" AS s
WHERE s.""UnmappedEnum"" = @__sad_0
LIMIT 2");
        }

        [Fact]
        public void Where_with_mapped_enum_parameter_downcasts_do_not_matter()
        {
            using var ctx = CreateContext();
            // ReSharper disable once ConvertToConstant.Local
            var sad = MappedEnum.Sad;
            var _ = ctx.SomeEntities.Single(e => (int)e.MappedEnum == (int)sad);

            AssertSql(
                @"@__sad_0='Sad' (DbType = Object)

SELECT s.""Id"", s.""ByteEnum"", s.""EnumValue"", s.""InferredEnum"", s.""MappedEnum"", s.""SchemaQualifiedEnum"", s.""UnmappedByteEnum"", s.""UnmappedEnum""
FROM test.""SomeEntities"" AS s
WHERE s.""MappedEnum"" = @__sad_0
LIMIT 2");
        }

        [Fact]
        public void Enum_ToString()
        {
            using var ctx = CreateContext();
            // Note we have to specify lower-case since the ADO layer applies naming transformations, not ideal.
            var _ = ctx.SomeEntities.Single(e => e.MappedEnum.ToString().Contains("sa"));

            AssertSql(
                @"SELECT s.""Id"", s.""ByteEnum"", s.""EnumValue"", s.""InferredEnum"", s.""MappedEnum"", s.""SchemaQualifiedEnum"", s.""UnmappedByteEnum"", s.""UnmappedEnum""
FROM test.""SomeEntities"" AS s
WHERE strpos(CAST(s.""MappedEnum"" AS text), 'sa') > 0
LIMIT 2");
        }

        [Fact]
        public void Where_byte_enum_array_contains_enum()
        {
            using var ctx = CreateContext();
            var values = new[] { ByteEnum.Sad };
            var result = ctx.SomeEntities.Single(e => values.Contains(e.ByteEnum));
            Assert.Equal(2, result.Id);

            AssertSql(
                @"@__values_0='Npgsql.EntityFrameworkCore.PostgreSQL.Query.EnumQueryTest+ByteEnum[]' (DbType = Object)

SELECT s.""Id"", s.""ByteEnum"", s.""EnumValue"", s.""InferredEnum"", s.""MappedEnum"", s.""SchemaQualifiedEnum"", s.""UnmappedByteEnum"", s.""UnmappedEnum""
FROM test.""SomeEntities"" AS s
WHERE s.""ByteEnum"" = ANY (@__values_0)
LIMIT 2");
        }

        [Fact]
        public void Where_unmapped_byte_enum_array_contains_enum()
        {
            using var ctx = CreateContext();
            var values = new[] { UnmappedByteEnum.Sad };
            var result = ctx.SomeEntities.Single(e => values.Contains(e.UnmappedByteEnum));
            Assert.Equal(2, result.Id);

            // Note: EF Core prints the parameter as a bytea, but it's actually a smallint[] (otherwise ANY would fail)
            AssertSql(
                @"@__values_0='0x01' (DbType = Object)

SELECT s.""Id"", s.""ByteEnum"", s.""EnumValue"", s.""InferredEnum"", s.""MappedEnum"", s.""SchemaQualifiedEnum"", s.""UnmappedByteEnum"", s.""UnmappedEnum""
FROM test.""SomeEntities"" AS s
WHERE s.""UnmappedByteEnum"" = ANY (@__values_0)
LIMIT 2");
        }

        #endregion

        #region Support

        protected EnumContext CreateContext() => Fixture.CreateContext();

        void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        void AssertContainsInSql(string expected)
            => Assert.Contains(expected, Fixture.TestSqlLoggerFactory.Sql);

        // ReSharper disable once UnusedMember.Local
        void AssertDoesNotContainInSql(string expected)
            => Assert.DoesNotContain(expected, Fixture.TestSqlLoggerFactory.Sql);

        public class EnumContext : PoolableDbContext
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Global
            public DbSet<SomeEnumEntity> SomeEntities { get; set; }

            static EnumContext()
            {
                NpgsqlConnection.GlobalTypeMapper.MapEnum<MappedEnum>("test.mapped_enum");
                NpgsqlConnection.GlobalTypeMapper.MapEnum<InferredEnum>("test.inferred_enum");
                NpgsqlConnection.GlobalTypeMapper.MapEnum<ByteEnum>("test.byte_enum");
                NpgsqlConnection.GlobalTypeMapper.MapEnum<SchemaQualifiedEnum>("test.schema_qualified_enum");
            }

            public EnumContext(DbContextOptions options) : base(options) {}

            protected override void OnModelCreating(ModelBuilder builder)
                => builder.HasPostgresEnum("mapped_enum", new[] { "happy", "sad" })
                          .HasPostgresEnum<InferredEnum>()
                          .HasPostgresEnum<ByteEnum>()
                          .HasDefaultSchema("test")
                          .HasPostgresEnum<SchemaQualifiedEnum>();

            public static void Seed(EnumContext context)
            {
                context.SomeEntities.AddRange(
                    new SomeEnumEntity
                    {
                        Id = 1,
                        MappedEnum = MappedEnum.Happy,
                        UnmappedEnum = UnmappedEnum.Happy,
                        InferredEnum = InferredEnum.Happy,
                        SchemaQualifiedEnum = SchemaQualifiedEnum.Happy,
                        ByteEnum = ByteEnum.Happy,
                        UnmappedByteEnum = UnmappedByteEnum.Happy,
                        EnumValue = (int)MappedEnum.Happy
                    },
                    new SomeEnumEntity
                    {
                        Id = 2,
                        MappedEnum = MappedEnum.Sad,
                        UnmappedEnum = UnmappedEnum.Sad,
                        InferredEnum = InferredEnum.Sad,
                        SchemaQualifiedEnum = SchemaQualifiedEnum.Sad,
                        ByteEnum = ByteEnum.Sad,
                        UnmappedByteEnum = UnmappedByteEnum.Sad,
                        EnumValue = (int)MappedEnum.Sad
                    });
                context.SaveChanges();
            }
        }

        public class SomeEnumEntity
        {
            public int Id { get; set; }
            public MappedEnum MappedEnum { get; set; }
            public UnmappedEnum UnmappedEnum { get; set; }
            public InferredEnum InferredEnum { get; set; }
            public SchemaQualifiedEnum SchemaQualifiedEnum { get; set; }
            public ByteEnum ByteEnum { get; set; }
            public UnmappedByteEnum UnmappedByteEnum { get; set; }
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
            [PgName("Happy (PgName)")]
            Happy,
            Sad
        }

        public enum ByteEnum : byte
        {
            Happy,
            Sad
        }

        public enum UnmappedByteEnum : byte
        {
            Happy,
            Sad
        }

        // ReSharper disable once ClassNeverInstantiated.Global
        public class EnumFixture : SharedStoreFixtureBase<EnumContext>
        {
            protected override string StoreName => "EnumQueryTest";
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

            protected override void Seed(EnumContext context) => EnumContext.Seed(context);
        }

        #endregion
    }
}
