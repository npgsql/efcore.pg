using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class CubeQueryNpgsqlTest : IClassFixture<CubeQueryNpgsqlTest.CubeQueryNpgqlFixture>
{
    public CubeQueryNpgqlFixture Fixture { get; }

    public CubeQueryNpgsqlTest(CubeQueryNpgqlFixture fixture)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
    }

    #region Operators

    [ConditionalFact]
    public void Contains_value()
    {
        using var context = CreateContext();
        var result = context.CubeTestEntities.Where(x => x.Cube.Contains(new NpgsqlCube(new[] { 0.0, 0.0, 0.0 })));
        var sql = result.ToQueryString();
        Assert.Equal(1, result.Single().Id);
    }

    #endregion

    public class CubeQueryNpgqlFixture : SharedStoreFixtureBase<CubeContext>
    {
        protected override string StoreName => "CubeQueryTest";
        protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
        protected override void Seed(CubeContext context) => CubeContext.Seed(context);
    }

    public class CubeContext : PoolableDbContext
    {
        public DbSet<CubeTestEntity> CubeTestEntities { get; set; }

        public CubeContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
            => builder.HasPostgresExtension("cube");

        public static void Seed(CubeContext context)
        {
            context.CubeTestEntities.AddRange(
                new CubeTestEntity
                {
                    Id = 1,
                    Cube = new NpgsqlCube(new[] { -1.0, -1.0, -1.0 }, new[] { 1.0, 1.0, 1.0 })
                },
                new CubeTestEntity
                {
                    Id = 2,
                    Cube = new NpgsqlCube(new []{ 1.0, 1.0, 1.0 })
                });

            context.SaveChanges();
        }
    }

    public class CubeTestEntity
    {
        public int Id { get; set; }

        public NpgsqlCube Cube { get; set; }
    }

    #region Helpers

    protected CubeContext CreateContext() => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    #endregion
}
