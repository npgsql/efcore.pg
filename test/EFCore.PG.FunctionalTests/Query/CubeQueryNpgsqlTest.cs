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
        var result = context.CubeTestEntities.Single(x => x.Cube.Contains(new NpgsqlCube(new[] { 0.0, 0.0, 0.0 })));
        Assert.Equal(1, result.Id);
        AssertSql("""
            SELECT c."Id", c."Cube"
            FROM "CubeTestEntities" AS c
            WHERE c."Cube" @> '(0,0,0)'::cube
            LIMIT 2
            """);
    }

    [ConditionalFact]
    public void Subset()
    {
        using var context = CreateContext();
        var result = context.CubeTestEntities.Where(x => x.Id == 1).Select(x => x.Cube.Subset(1)).Single();
        Assert.Equal(new NpgsqlCube(-1, 1), result);
        AssertSql("""
            SELECT cube_subset(c."Cube", ARRAY[1]::integer[])
            FROM "CubeTestEntities" AS c
            WHERE c."Id" = 1
            LIMIT 2
            """);
    }

    [ConditionalFact]
    public void Dimensions()
    {
        using var context = CreateContext();
        var result = context.CubeTestEntities.Where(x => x.Id == 1).Select(x => x.Cube.Dimensions).Single();
        Assert.Equal(3, result);
        AssertSql("""
            SELECT cube_dim(c."Cube")
            FROM "CubeTestEntities" AS c
            WHERE c."Id" = 1
            LIMIT 2
            """);
    }

    [ConditionalFact]
    public void Is_point()
    {
        using var context = CreateContext();
        var result = context.CubeTestEntities.Single(x => x.Cube.Point);
        Assert.Equal(2, result.Id);
        AssertSql("""
            SELECT c."Id", c."Cube"
            FROM "CubeTestEntities" AS c
            WHERE cube_is_point(c."Cube")
            LIMIT 2
            """);
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
