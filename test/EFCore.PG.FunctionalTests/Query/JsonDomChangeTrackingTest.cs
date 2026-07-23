using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Microsoft.EntityFrameworkCore.Query;

public class JsonDomChangeTrackingTest : IClassFixture<JsonDomChangeTrackingTest.JsonDomChangeTrackingFixture>
{
    private JsonDomChangeTrackingFixture Fixture { get; }

    public JsonDomChangeTrackingTest(JsonDomChangeTrackingFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [Theory]
    [InlineData("""{"Name":"John","Age":25}""", true)]
    [InlineData("""{"Age":25.00,"Name":"Joe"}""", false)]
    public async Task SaveChanges_jsonb_document(string json, bool isModified)
    {
        await using var ctx = CreateContext();
        await ctx.Database.CreateExecutionStrategy().ExecuteAsync(
            ctx, async context =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                var entity = await context.JsonbEntities.SingleAsync(e => e.Id == 1);
                entity.CustomerDocument = JsonDocument.Parse(json);
                Fixture.TestSqlLoggerFactory.Clear();
                await context.SaveChangesAsync();

                if (isModified)
                {
                    AssertSql(
                        """
@p1='1'
@p0='System.Text.Json.JsonDocument' (DbType = Object)

UPDATE "JsonbEntities" SET "CustomerDocument" = @p0
WHERE "Id" = @p1;
""");
                }
                else
                {
                    AssertEmptySql();
                }
            });
    }

    [Theory]
    [InlineData("""{"Name":"Joe","Age":26}""", true)]
    [InlineData("""{"Age":25.00,"Name":"Joe"}""", false)]
    public async Task SaveChanges_json_document(string json, bool isModified)
    {
        await using var ctx = CreateContext();
        await ctx.Database.CreateExecutionStrategy().ExecuteAsync(
            ctx, async context =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                var entity = await context.JsonEntities.SingleAsync(e => e.Id == 1);
                entity.CustomerDocument = JsonDocument.Parse(json);
                Fixture.TestSqlLoggerFactory.Clear();
                await context.SaveChangesAsync();

                if (isModified)
                {
                    AssertSql(
                        """
@p1='1'
@p0='System.Text.Json.JsonDocument' (DbType = Object)

UPDATE "JsonEntities" SET "CustomerDocument" = @p0
WHERE "Id" = @p1;
""");
                }
                else
                {
                    AssertEmptySql();
                }
            });
    }

    [Theory]
    [InlineData("""{"Name":"John","Age":25}""", true)]
    [InlineData("""{"Age":25.00,"Name":"Joe"}""", false)]
    public async Task SaveChanges_jsonb_element(string json, bool isModified)
    {
        await using var ctx = CreateContext();
        await ctx.Database.CreateExecutionStrategy().ExecuteAsync(
            ctx, async context =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                var entity = await context.JsonbEntities.SingleAsync(e => e.Id == 1);
                entity.CustomerElement = JsonElement.Parse(json);
                Fixture.TestSqlLoggerFactory.Clear();
                await context.SaveChangesAsync();

                if (isModified)
                {
                    AssertSql(
                        """
@p1='1'
@p0='{"Name":"John","Age":25}' (DbType = Object)

UPDATE "JsonbEntities" SET "CustomerElement" = @p0
WHERE "Id" = @p1;
""");
                }
                else
                {
                    AssertEmptySql();
                }
            });
    }

    [Theory]
    [InlineData("""{"Name":"Joe","Age":26}""", true)]
    [InlineData("""{"Age":25.00,"Name":"Joe"}""", false)]
    public async Task SaveChanges_json_element(string json, bool isModified)
    {
        await using var ctx = CreateContext();
        await ctx.Database.CreateExecutionStrategy().ExecuteAsync(
            ctx, async context =>
            {
                await using var transaction = await context.Database.BeginTransactionAsync();

                var entity = await context.JsonEntities.SingleAsync(e => e.Id == 1);
                entity.CustomerElement = JsonElement.Parse(json);
                Fixture.TestSqlLoggerFactory.Clear();
                await context.SaveChangesAsync();

                if (isModified)
                {
                    AssertSql(
                        """
@p1='1'
@p0='{"Name":"Joe","Age":26}' (DbType = Object)

UPDATE "JsonEntities" SET "CustomerElement" = @p0
WHERE "Id" = @p1;
""");
                }
                else
                {
                    AssertEmptySql();
                }
            });
    }

    [Fact]
    public async Task DetectChanges_jsonb_undefined_element_no_throw()
    {
        await using var ctx = CreateContext();

        var entity = await ctx.JsonbEntities.SingleAsync(e => e.Id == 1);
        entity.CustomerElement = new JsonElement();
        ctx.JsonbEntities.Add(
            new JsonbEntity
            {
                Id = 2,
                CustomerElement = new JsonElement(),
                CustomerDocument = null
            });

        ctx.ChangeTracker.DetectChanges();
    }

    [Fact]
    public async Task DetectChanges_json_undefined_element_no_throw()
    {
        await using var ctx = CreateContext();

        var entity = await ctx.JsonEntities.SingleAsync(e => e.Id == 1);
        entity.CustomerElement = new JsonElement();
        ctx.JsonEntities.Add(
            new JsonEntity
            {
                Id = 2,
                CustomerElement = new JsonElement(),
                CustomerDocument = null
            });

        ctx.ChangeTracker.DetectChanges();
    }

    #region Support

    protected JsonDomChangeTrackingContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    private void AssertEmptySql()
        => Assert.Empty(Fixture.TestSqlLoggerFactory.SqlStatements);

    public class JsonDomChangeTrackingContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<JsonbEntity> JsonbEntities { get; set; }
        public DbSet<JsonEntity> JsonEntities { get; set; }

        public static async Task SeedAsync(JsonDomChangeTrackingContext context)
        {
            var customer = CreateCustomer();

            context.JsonbEntities.Add(
                new JsonbEntity
                {
                    Id = 1,
                    CustomerDocument = customer,
                    CustomerElement = customer.RootElement
                });
            context.JsonEntities.Add(
                new JsonEntity
                {
                    Id = 1,
                    CustomerDocument = customer,
                    CustomerElement = customer.RootElement
                });

            await context.SaveChangesAsync();

            static JsonDocument CreateCustomer()
                => JsonDocument.Parse("""{"Name":"Joe","Age":25}""");
        }
    }

    public class JsonbEntity
    {
        public required int Id { get; set; }

        public required JsonDocument? CustomerDocument { get; set; }
        public required JsonElement CustomerElement { get; set; }
    }

    public class JsonEntity
    {
        public required int Id { get; set; }

        [Column(TypeName = "json")]
        public required JsonDocument? CustomerDocument { get; set; }

        [Column(TypeName = "json")]
        public required JsonElement CustomerElement { get; set; }
    }

    public class JsonDomChangeTrackingFixture : SharedStoreFixtureBase<JsonDomChangeTrackingContext>
    {
        protected override string StoreName
            => "JsonDomChangeTrackingTest";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override Task SeedAsync(JsonDomChangeTrackingContext context)
            => JsonDomChangeTrackingContext.SeedAsync(context);
    }

    #endregion
}
