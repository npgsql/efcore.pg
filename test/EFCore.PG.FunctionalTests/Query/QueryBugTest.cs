using System.ComponentModel.DataAnnotations.Schema;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

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
    public void Bug920()
    {
        using var _ = CreateDatabase920();
        using var context = new Bug920Context(_options);
        context.Entities.Add(new Bug920Entity { Enum = Bug920Enum.Two });
        context.SaveChanges();
    }

    private NpgsqlTestStore CreateDatabase920()
        => CreateTestStore(() => new Bug920Context(_options), _ => ClearLog());

    public enum Bug920Enum { One, Two }

    public class Bug920Entity
    {
        public int Id { get; set; }

        [Column(TypeName = "char(3)")]
        public Bug920Enum Enum { get; set; }
    }

    private class Bug920Context : DbContext
    {
        public Bug920Context(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Bug920Entity> Entities { get; set; }
    }

    #endregion Bug920

    private DbContextOptions _options;

    private NpgsqlTestStore CreateTestStore<TContext>(
        Func<TContext> contextCreator,
        Action<TContext> contextInitializer)
        where TContext : DbContext, IDisposable
    {
        var testStore = NpgsqlTestStore.CreateInitialized("QueryBugsTest");

        _options = Fixture.CreateOptions(testStore);

        using var context = contextCreator();
        context.Database.EnsureCreatedResiliently();
        contextInitializer?.Invoke(context);
        return testStore;
    }

    protected void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
