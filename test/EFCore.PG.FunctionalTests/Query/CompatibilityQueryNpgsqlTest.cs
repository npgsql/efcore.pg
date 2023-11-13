using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class CompatibilityQueryNpgsqlTest : IClassFixture<CompatibilityQueryNpgsqlTest.CompatibilityQueryNpgsqlFixture>
{
    private CompatibilityQueryNpgsqlFixture Fixture { get; }

    // ReSharper disable once UnusedParameter.Local
    public CompatibilityQueryNpgsqlTest(CompatibilityQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact]
    public async Task Array_contains_is_not_parameterized_with_array_on_redshift()
    {
        var ctx = CreateRedshiftContext();

        var numbers = new[] { 8, 9 };
        var result = await ctx.TestEntities.Where(e => numbers.Contains(e.SomeInt)).SingleAsync();
        Assert.Equal(1, result.Id);

        AssertSql(
            """
SELECT t."Id", t."SomeInt"
FROM "TestEntities" AS t
WHERE t."SomeInt" IN (8, 9)
LIMIT 2
""");
    }

    #region Support

    private CompatibilityContext CreateContext(Version postgresVersion = null)
        => Fixture.CreateContext(postgresVersion);

    private CompatibilityContext CreateRedshiftContext()
        => Fixture.CreateRedshiftContext();

    public class CompatibilityQueryNpgsqlFixture : FixtureBase, IDisposable, IAsyncLifetime
    {
        private TestStore _testStore;

        private const string StoreName = "CompatibilityTest";
        private readonly ListLoggerFactory _listLoggerFactory = NpgsqlTestStoreFactory.Instance.CreateListLoggerFactory(_ => false);

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)_listLoggerFactory;

        public virtual CompatibilityContext CreateContext()
            => CreateContext(null);

        public virtual CompatibilityContext CreateContext(Version postgresVersion)
        {
            var builder = new DbContextOptionsBuilder();
            _testStore.AddProviderOptions(builder);
            builder
                .UseNpgsql(o => o.SetPostgresVersion(postgresVersion))
                .UseLoggerFactory(_listLoggerFactory);
            return new CompatibilityContext(builder.Options);
        }

        public virtual CompatibilityContext CreateRedshiftContext()
        {
            var builder = new DbContextOptionsBuilder();
            _testStore.AddProviderOptions(builder);
            builder
                .UseNpgsql(o => o.UseRedshift())
                .UseLoggerFactory(_listLoggerFactory);
            return new CompatibilityContext(builder.Options);
        }

        public virtual Task InitializeAsync()
        {
            _testStore = NpgsqlTestStoreFactory.Instance.GetOrCreate(StoreName);
            _testStore.Initialize(null, CreateContext, c => CompatibilityContext.Seed((CompatibilityContext)c));
            return Task.CompletedTask;
        }

        // Called after DisposeAsync
        public virtual void Dispose()
        {
        }

        public virtual Task DisposeAsync()
            => _testStore.DisposeAsync();
    }

    public class CompatibilityTestEntity
    {
        public int Id { get; set; }
        public int SomeInt { get; set; }
    }

    public class CompatibilityContext : DbContext
    {
        public DbSet<CompatibilityTestEntity> TestEntities { get; set; }

        public CompatibilityContext(DbContextOptions options)
            : base(options)
        {
        }

        public static void Seed(CompatibilityContext context)
        {
            context.TestEntities.AddRange(
                new CompatibilityTestEntity { Id = 1, SomeInt = 8 },
                new CompatibilityTestEntity { Id = 2, SomeInt = 10 });
            context.SaveChanges();
        }
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    #endregion Support
}
