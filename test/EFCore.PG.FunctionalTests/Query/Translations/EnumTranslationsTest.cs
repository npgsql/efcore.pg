using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class EnumTranslationsTest : QueryTestBase<EnumTranslationsTest.EnumFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public EnumTranslationsTest(EnumFixture fixture, ITestOutputHelper testOutputHelper)
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
SELECT s."Id", s."ByteEnum", s."EnumValue", s."InferredEnum", s."MappedEnum", s."SchemaQualifiedEnum", s."UnmappedByteEnum", s."UnmappedEnum", s."UppercaseNamedEnum"
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
SELECT s."Id", s."ByteEnum", s."EnumValue", s."InferredEnum", s."MappedEnum", s."SchemaQualifiedEnum", s."UnmappedByteEnum", s."UnmappedEnum", s."UppercaseNamedEnum"
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
@sad='Sad' (DbType = Object)

SELECT s."Id", s."ByteEnum", s."EnumValue", s."InferredEnum", s."MappedEnum", s."SchemaQualifiedEnum", s."UnmappedByteEnum", s."UnmappedEnum", s."UppercaseNamedEnum"
FROM test."SomeEntities" AS s
WHERE s."MappedEnum" = @sad
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
@sad='1'

SELECT s."Id", s."ByteEnum", s."EnumValue", s."InferredEnum", s."MappedEnum", s."SchemaQualifiedEnum", s."UnmappedByteEnum", s."UnmappedEnum", s."UppercaseNamedEnum"
FROM test."SomeEntities" AS s
WHERE s."UnmappedEnum" = @sad
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
@sad='1'

SELECT s."Id", s."ByteEnum", s."EnumValue", s."InferredEnum", s."MappedEnum", s."SchemaQualifiedEnum", s."UnmappedByteEnum", s."UnmappedEnum", s."UppercaseNamedEnum"
FROM test."SomeEntities" AS s
WHERE s."UnmappedEnum" = @sad
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
@sad='Sad' (DbType = Object)

SELECT s."Id", s."ByteEnum", s."EnumValue", s."InferredEnum", s."MappedEnum", s."SchemaQualifiedEnum", s."UnmappedByteEnum", s."UnmappedEnum", s."UppercaseNamedEnum"
FROM test."SomeEntities" AS s
WHERE s."MappedEnum" = @sad
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
SELECT s."Id", s."ByteEnum", s."EnumValue", s."InferredEnum", s."MappedEnum", s."SchemaQualifiedEnum", s."UnmappedByteEnum", s."UnmappedEnum", s."UppercaseNamedEnum"
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
@values='0x01' (DbType = Object)

SELECT s."Id", s."ByteEnum", s."EnumValue", s."InferredEnum", s."MappedEnum", s."SchemaQualifiedEnum", s."UnmappedByteEnum", s."UnmappedEnum", s."UppercaseNamedEnum"
FROM test."SomeEntities" AS s
WHERE s."ByteEnum" = ANY (@values)
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
@values='0x01' (DbType = Object)

SELECT s."Id", s."ByteEnum", s."EnumValue", s."InferredEnum", s."MappedEnum", s."SchemaQualifiedEnum", s."UnmappedByteEnum", s."UnmappedEnum", s."UppercaseNamedEnum"
FROM test."SomeEntities" AS s
WHERE s."UnmappedByteEnum" = ANY (@values)
""");
    }

    [ConditionalTheory] // #3433
    [MemberData(nameof(IsAsyncData))]
    public async Task Where_uppercase_enum_array_contains_enum(bool async)
    {
        await using var ctx = CreateContext();

        List<UppercaseNamedEnum> values = [UppercaseNamedEnum.Sad];
        await AssertQuery(
            async,
            ss => ss.Set<SomeEnumEntity>().Where(e => values.Contains(e.UppercaseNamedEnum)));

        AssertSql(
            """
@values={ 'Sad' } (DbType = Object)

SELECT s."Id", s."ByteEnum", s."EnumValue", s."InferredEnum", s."MappedEnum", s."SchemaQualifiedEnum", s."UnmappedByteEnum", s."UnmappedEnum", s."UppercaseNamedEnum"
FROM test."SomeEntities" AS s
WHERE s."UppercaseNamedEnum" = ANY (@values)
""");
    }

    #endregion

    #region Support

    protected EnumContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    public class EnumContext(DbContextOptions options) : PoolableDbContext(options)
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public DbSet<SomeEnumEntity> SomeEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
            => builder.HasDefaultSchema("test");

        public static async Task SeedAsync(EnumContext context)
        {
            context.AddRange(EnumData.CreateSomeEnumEntities());
            await context.SaveChangesAsync();
        }
    }

    public class SomeEnumEntity
    {
        public int Id { get; set; }
        public MappedEnum MappedEnum { get; set; }
        public UnmappedEnum UnmappedEnum { get; set; }
        public InferredEnum InferredEnum { get; set; }
        public SchemaQualifiedEnum SchemaQualifiedEnum { get; set; }
        public UppercaseNamedEnum UppercaseNamedEnum { get; set; }
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

    public enum UppercaseNamedEnum
    {
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
    public class EnumFixture : SharedStoreFixtureBase<EnumContext>, IQueryFixtureBase, ITestSqlLoggerFactory
    {
        protected override string StoreName
            => "EnumQueryTest";

        // We instruct the test store to pass a connection string to UseNpgsql() instead of a DbConnection - that's required to allow
        // EF's UseNodaTime() to function properly and instantiate an NpgsqlDataSource internally.
        protected override ITestStoreFactory TestStoreFactory
            => new NpgsqlTestStoreFactory(useConnectionString: true);

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            var optionsBuilder = base.AddOptions(builder);

            new NpgsqlDbContextOptionsBuilder(optionsBuilder)
                .MapEnum<MappedEnum>("mapped_enum", "test")
                .MapEnum<InferredEnum>("inferred_enum", "test")
                .MapEnum<ByteEnum>("byte_enum", "test")
                .MapEnum<SchemaQualifiedEnum>("schema_qualified_enum", "test")
                .MapEnum<UppercaseNamedEnum>("UpperCaseEnum", "test");

            return optionsBuilder;
        }

        private EnumData? _expectedData;

        protected override Task SeedAsync(EnumContext context)
            => EnumContext.SeedAsync(context);

        public Func<DbContext> GetContextCreator()
            => CreateContext;

        public ISetSource GetExpectedData()
            => _expectedData ??= new EnumData();

        public IReadOnlyDictionary<Type, object> EntitySorters
            => new Dictionary<Type, Func<object, object?>> { { typeof(SomeEnumEntity), e => ((SomeEnumEntity)e)?.Id } }
                .ToDictionary(e => e.Key, e => (object)e.Value);

        public IReadOnlyDictionary<Type, object> EntityAsserters
            => new Dictionary<Type, Action<object, object>>
            {
                {
                    typeof(SomeEnumEntity), (e, a) =>
                    {
                        Assert.Equal(e is null, a is null);

                        if (e is not null && a is not null)
                        {
                            var ee = (SomeEnumEntity)e;
                            var aa = (SomeEnumEntity)a;

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.MappedEnum, aa.MappedEnum);
                            Assert.Equal(ee.UnmappedEnum, aa.UnmappedEnum);
                            Assert.Equal(ee.InferredEnum, aa.InferredEnum);
                            Assert.Equal(ee.SchemaQualifiedEnum, aa.SchemaQualifiedEnum);
                            Assert.Equal(ee.UppercaseNamedEnum, aa.UppercaseNamedEnum);
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
                    UppercaseNamedEnum = UppercaseNamedEnum.Happy,
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
                    UppercaseNamedEnum = UppercaseNamedEnum.Sad,
                    ByteEnum = ByteEnum.Sad,
                    UnmappedByteEnum = UnmappedByteEnum.Sad,
                    EnumValue = (int)MappedEnum.Sad
                }
            };
    }

    #endregion
}
