using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class EnumQueryTest : QueryTestBase<EnumQueryTest.EnumFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public EnumQueryTest(EnumFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
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

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Where_with_constant(bool async)
    {
        await using var ctx = CreateContext();

        await AssertQuery(
            async,
            ss => ss.Set<SomeEnumEntity>().Where(e => e.MappedEnum == MappedEnum.Sad));

        AssertSql(
            """
SELECT s."Id", s."ByteEnum", s."EnumValue", s."InferredEnum", s."MappedEnum", s."SchemaQualifiedEnum", s."UnmappedByteEnum", s."UnmappedEnum"
FROM test."SomeEntities" AS s
WHERE s."MappedEnum" = 'sad'::test.mapped_enum
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Where_with_constant_schema_qualified(bool async)
    {
        await using var ctx = CreateContext();

        await AssertQuery(
            async,
            ss => ss.Set<SomeEnumEntity>().Where(e => e.SchemaQualifiedEnum == SchemaQualifiedEnum.Happy));

        AssertSql(
            """
SELECT s."Id", s."ByteEnum", s."EnumValue", s."InferredEnum", s."MappedEnum", s."SchemaQualifiedEnum", s."UnmappedByteEnum", s."UnmappedEnum"
FROM test."SomeEntities" AS s
WHERE s."SchemaQualifiedEnum" = 'Happy (PgName)'::test.schema_qualified_enum
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Where_with_parameter(bool async)
    {
        await using var ctx = CreateContext();

        var sad = MappedEnum.Sad;
        await AssertQuery(
            async,
            ss => ss.Set<SomeEnumEntity>().Where(e => e.MappedEnum == sad));

        AssertSql(
            """
@__sad_0='Sad' (DbType = Object)

SELECT s."Id", s."ByteEnum", s."EnumValue", s."InferredEnum", s."MappedEnum", s."SchemaQualifiedEnum", s."UnmappedByteEnum", s."UnmappedEnum"
FROM test."SomeEntities" AS s
WHERE s."MappedEnum" = @__sad_0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Where_with_unmapped_enum_parameter_downcasts_are_implicit(bool async)
    {
        await using var ctx = CreateContext();

        var sad = UnmappedEnum.Sad;
        await AssertQuery(
            async,
            ss => ss.Set<SomeEnumEntity>().Where(e => e.UnmappedEnum == sad));

        AssertSql(
            """
@__sad_0='1'

SELECT s."Id", s."ByteEnum", s."EnumValue", s."InferredEnum", s."MappedEnum", s."SchemaQualifiedEnum", s."UnmappedByteEnum", s."UnmappedEnum"
FROM test."SomeEntities" AS s
WHERE s."UnmappedEnum" = @__sad_0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Where_with_unmapped_enum_parameter_downcasts_do_not_matter(bool async)
    {
        await using var ctx = CreateContext();

        var sad = UnmappedEnum.Sad;
        await AssertQuery(
            async,
            ss => ss.Set<SomeEnumEntity>().Where(e => (int)e.UnmappedEnum == (int)sad));

        AssertSql(
            """
@__sad_0='1'

SELECT s."Id", s."ByteEnum", s."EnumValue", s."InferredEnum", s."MappedEnum", s."SchemaQualifiedEnum", s."UnmappedByteEnum", s."UnmappedEnum"
FROM test."SomeEntities" AS s
WHERE s."UnmappedEnum" = @__sad_0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Where_with_mapped_enum_parameter_downcasts_do_not_matter(bool async)
    {
        await using var ctx = CreateContext();

        var sad = MappedEnum.Sad;
        await AssertQuery(
            async,
            ss => ss.Set<SomeEnumEntity>().Where(e => (int)e.MappedEnum == (int)sad));

        AssertSql(
            """
@__sad_0='Sad' (DbType = Object)

SELECT s."Id", s."ByteEnum", s."EnumValue", s."InferredEnum", s."MappedEnum", s."SchemaQualifiedEnum", s."UnmappedByteEnum", s."UnmappedEnum"
FROM test."SomeEntities" AS s
WHERE s."MappedEnum" = @__sad_0
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Enum_ToString(bool async)
    {
        await using var ctx = CreateContext();

        await AssertQuery(
            async,
            ss => ss.Set<SomeEnumEntity>().Where(e => e.MappedEnum.ToString().Contains("sa")),
            ss => ss.Set<SomeEnumEntity>().Where(e => e.MappedEnum.ToString().Contains("Sa")));

        AssertSql(
            """
SELECT s."Id", s."ByteEnum", s."EnumValue", s."InferredEnum", s."MappedEnum", s."SchemaQualifiedEnum", s."UnmappedByteEnum", s."UnmappedEnum"
FROM test."SomeEntities" AS s
WHERE s."MappedEnum"::text LIKE '%sa%'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Where_byte_enum_array_contains_enum(bool async)
    {
        await using var ctx = CreateContext();

        var values = new[] { ByteEnum.Sad };
        await AssertQuery(
            async,
            ss => ss.Set<SomeEnumEntity>().Where(e => values.Contains(e.ByteEnum)));

        AssertSql(
            """
@__values_0='0x01' (DbType = Object)

SELECT s."Id", s."ByteEnum", s."EnumValue", s."InferredEnum", s."MappedEnum", s."SchemaQualifiedEnum", s."UnmappedByteEnum", s."UnmappedEnum"
FROM test."SomeEntities" AS s
WHERE s."ByteEnum" = ANY (@__values_0)
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Where_unmapped_byte_enum_array_contains_enum(bool async)
    {
        await using var ctx = CreateContext();

        var values = new[] { UnmappedByteEnum.Sad };
        await AssertQuery(
            async,
            ss => ss.Set<SomeEnumEntity>().Where(e => values.Contains(e.UnmappedByteEnum)));

        AssertSql(
            """
@__values_0='0x01' (DbType = Object)

SELECT s."Id", s."ByteEnum", s."EnumValue", s."InferredEnum", s."MappedEnum", s."SchemaQualifiedEnum", s."UnmappedByteEnum", s."UnmappedEnum"
FROM test."SomeEntities" AS s
WHERE s."UnmappedByteEnum" = ANY (@__values_0)
""");
    }

    #endregion

    #region Support

    protected EnumContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class EnumContext : PoolableDbContext
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DbSet<SomeEnumEntity> SomeEntities { get; set; }

        public EnumContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
            => builder
                .HasPostgresEnum("mapped_enum", new[] { "happy", "sad" })
                .HasPostgresEnum<InferredEnum>()
                .HasPostgresEnum<ByteEnum>()
                .HasDefaultSchema("test")
                .HasPostgresEnum<SchemaQualifiedEnum>();

        public static void Seed(EnumContext context)
        {
            context.AddRange(EnumData.CreateSomeEnumEntities());
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
    }

    public enum UnmappedEnum
    {
        Happy,
        Sad
    }

    public enum InferredEnum
    {
        Happy,
        Sad
    }

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
    public class EnumFixture : SharedStoreFixtureBase<EnumContext>, IQueryFixtureBase
    {
        protected override string StoreName
            => "EnumQueryTest";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        static EnumFixture()
        {
#pragma warning disable CS0618 // NpgsqlConnection.GlobalTypeMapper is obsolete
            NpgsqlConnection.GlobalTypeMapper.MapEnum<MappedEnum>("test.mapped_enum");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<InferredEnum>("test.inferred_enum");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<ByteEnum>("test.byte_enum");
            NpgsqlConnection.GlobalTypeMapper.MapEnum<SchemaQualifiedEnum>("test.schema_qualified_enum");
#pragma warning restore CS0618
        }

        private EnumData _expectedData;

        protected override void Seed(EnumContext context)
            => EnumContext.Seed(context);

        public Func<DbContext> GetContextCreator()
            => CreateContext;

        public ISetSource GetExpectedData()
            => _expectedData ??= new EnumData();

        public IReadOnlyDictionary<Type, object> EntitySorters
            => new Dictionary<Type, Func<object, object>> { { typeof(SomeEnumEntity), e => ((SomeEnumEntity)e)?.Id } }
                .ToDictionary(e => e.Key, e => (object)e.Value);

        public IReadOnlyDictionary<Type, object> EntityAsserters
            => new Dictionary<Type, Action<object, object>>
            {
                {
                    typeof(SomeEnumEntity), (e, a) =>
                    {
                        Assert.Equal(e is null, a is null);
                        if (a is not null)
                        {
                            var ee = (SomeEnumEntity)e;
                            var aa = (SomeEnumEntity)a;

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.MappedEnum, aa.MappedEnum);
                            Assert.Equal(ee.UnmappedEnum, aa.UnmappedEnum);
                            Assert.Equal(ee.InferredEnum, aa.InferredEnum);
                            Assert.Equal(ee.SchemaQualifiedEnum, aa.SchemaQualifiedEnum);
                            Assert.Equal(ee.ByteEnum, aa.ByteEnum);
                            Assert.Equal(ee.UnmappedByteEnum, aa.UnmappedByteEnum);
                            Assert.Equal(ee.EnumValue, aa.EnumValue);
                        }
                    }
                }
            }.ToDictionary(e => e.Key, e => (object)e.Value);
    }

    protected class EnumData : ISetSource
    {
        public IReadOnlyList<SomeEnumEntity> SomeEnumEntities { get; } = CreateSomeEnumEntities();

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
            => typeof(TEntity) == typeof(SomeEnumEntity)
                ? (IQueryable<TEntity>)SomeEnumEntities.AsQueryable()
                : throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));

        public static IReadOnlyList<SomeEnumEntity> CreateSomeEnumEntities()
            => new List<SomeEnumEntity>
            {
                new()
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
                new()
                {
                    Id = 2,
                    MappedEnum = MappedEnum.Sad,
                    UnmappedEnum = UnmappedEnum.Sad,
                    InferredEnum = InferredEnum.Sad,
                    SchemaQualifiedEnum = SchemaQualifiedEnum.Sad,
                    ByteEnum = ByteEnum.Sad,
                    UnmappedByteEnum = UnmappedByteEnum.Sad,
                    EnumValue = (int)MappedEnum.Sad
                }
            };
    }

    #endregion
}
