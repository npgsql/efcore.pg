using System.ComponentModel.DataAnnotations.Schema;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class QueryBugsTest : IClassFixture<NpgsqlFixture>
{
    // ReSharper disable once UnusedParameter.Local
    public QueryBugsTest(NpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected NpgsqlFixture Fixture { get; }

    #region Bug920

    [Fact]
    public async Task Bug920()
    {
        await using var _ = await CreateDatabase920Async();
        using var context = new Bug920Context(_options);
        context.Entities.Add(new Bug920Entity { Enum = Bug920Enum.Two });
        context.SaveChanges();
    }

    private Task<NpgsqlTestStore> CreateDatabase920Async()
        => CreateTestStoreAsync(() => new Bug920Context(_options), _ => ClearLog());

    public enum Bug920Enum { One, Two }

    public class Bug920Entity
    {
        public int Id { get; set; }

        [Column(TypeName = "char(3)")]
        public Bug920Enum Enum { get; set; }
    }

    private class Bug920Context(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Bug920Entity> Entities { get; set; }
    }

    #endregion Bug920

    private DbContextOptions _options;

    private async Task<NpgsqlTestStore> CreateTestStoreAsync<TContext>(
        Func<TContext> contextCreator,
        Action<TContext> contextInitializer)
        where TContext : DbContext, IDisposable
    {
        var testStore = await NpgsqlTestStore.CreateInitializedAsync("QueryBugsTest");

        _options = Fixture.CreateOptions(testStore);

        await using var context = contextCreator();
        await context.Database.EnsureCreatedResilientlyAsync();
        contextInitializer?.Invoke(context);
        return testStore;
    }

    protected void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
