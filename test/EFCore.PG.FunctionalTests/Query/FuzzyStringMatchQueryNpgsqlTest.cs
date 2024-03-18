using System.ComponentModel.DataAnnotations;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

/// <summary>
///     Provides unit tests for the fuzzystrmatch module function translations.
/// </summary>
/// <remarks>
///     See: https://www.postgresql.org/docs/current/fuzzystrmatch.html
/// </remarks>
public class FuzzyStringMatchQueryNpgsqlTest : IClassFixture<FuzzyStringMatchQueryNpgsqlTest.FuzzyStringMatchQueryNpgsqlFixture>
{
    private FuzzyStringMatchQueryNpgsqlFixture Fixture { get; }

    // ReSharper disable once UnusedParameter.Local
    public FuzzyStringMatchQueryNpgsqlTest(FuzzyStringMatchQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region FunctionTests

    [Fact]
    public void FuzzyStringMatchSoundex()
    {
        using var context = CreateContext();
        var _ = context.FuzzyStringMatchTestEntities
            .Select(x => EF.Functions.FuzzyStringMatchSoundex(x.Text))
            .ToArray();

        AssertContainsSql(@"soundex(f.""Text"")");
    }

    [Fact]
    public void FuzzyStringMatchDifference()
    {
        using var context = CreateContext();
        var _ = context.FuzzyStringMatchTestEntities
            .Select(x => EF.Functions.FuzzyStringMatchDifference(x.Text, "target"))
            .ToArray();

        AssertContainsSql(@"difference(f.""Text"", 'target')");
    }

    [Fact]
    public void FuzzyStringMatchLevenshtein()
    {
        using var context = CreateContext();
        var _ = context.FuzzyStringMatchTestEntities
            .Select(x => EF.Functions.FuzzyStringMatchLevenshtein(x.Text, "target"))
            .ToArray();

        AssertContainsSql(@"levenshtein(f.""Text"", 'target')");
    }

    [Fact]
    public void FuzzyStringMatchLevenshtein_With_Costs()
    {
        using var context = CreateContext();
        var _ = context.FuzzyStringMatchTestEntities
            .Select(x => EF.Functions.FuzzyStringMatchLevenshtein(x.Text, "target", 1, 2, 3))
            .ToArray();

        AssertContainsSql(@"levenshtein(f.""Text"", 'target', 1, 2, 3)");
    }

    [Fact]
    public void FuzzyStringMatchLevenshteinLessEqual()
    {
        using var context = CreateContext();
        var _ = context.FuzzyStringMatchTestEntities
            .Select(x => EF.Functions.FuzzyStringMatchLevenshteinLessEqual(x.Text, "target", 5))
            .ToArray();

        AssertContainsSql(@"levenshtein_less_equal(f.""Text"", 'target', 5)");
    }

    [Fact]
    public void FuzzyStringMatchLevenshteinLessEqual_With_Costs()
    {
        using var context = CreateContext();
        var _ = context.FuzzyStringMatchTestEntities
            .Select(x => EF.Functions.FuzzyStringMatchLevenshteinLessEqual(x.Text, "target", 1, 2, 3, 5))
            .ToArray();

        AssertContainsSql(@"levenshtein_less_equal(f.""Text"", 'target', 1, 2, 3, 5)");
    }

    [Fact]
    public void FuzzyStringMatchMetaphone()
    {
        using var context = CreateContext();
        var _ = context.FuzzyStringMatchTestEntities
            .Select(x => EF.Functions.FuzzyStringMatchMetaphone(x.Text, 6))
            .ToArray();

        AssertContainsSql(@"metaphone(f.""Text"", 6)");
    }

    [Fact]
    public void FuzzyStringMatchDoubleMetaphone()
    {
        using var context = CreateContext();
        var _ = context.FuzzyStringMatchTestEntities
            .Select(x => EF.Functions.FuzzyStringMatchDoubleMetaphone(x.Text))
            .ToArray();

        AssertContainsSql(@"dmetaphone(f.""Text"")");
    }

    [Fact]
    public void FuzzyStringMatchDoubleMetaphoneAlt()
    {
        using var context = CreateContext();
        var _ = context.FuzzyStringMatchTestEntities
            .Select(x => EF.Functions.FuzzyStringMatchDoubleMetaphoneAlt(x.Text))
            .ToArray();

        AssertContainsSql(@"dmetaphone_alt(f.""Text"")");
    }

    #endregion

    #region Fixtures

    /// <summary>
    ///     Represents a fixture suitable for testing fuzzy string match functions.
    /// </summary>
    public class FuzzyStringMatchQueryNpgsqlFixture : SharedStoreFixtureBase<FuzzyStringMatchContext>
    {
        protected override string StoreName
            => "FuzzyStringMatchQueryTest";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override void Seed(FuzzyStringMatchContext context)
            => FuzzyStringMatchContext.Seed(context);
    }

    /// <summary>
    ///     Represents an entity suitable for testing fuzzy string match functions.
    /// </summary>
    public class FuzzyStringMatchTestEntity
    {
        // ReSharper disable once UnusedMember.Global
        /// <summary>
        ///     The primary key.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        ///     Some text.
        /// </summary>
        public string Text { get; set; }
    }

    /// <summary>
    ///     Represents a database suitable for testing fuzzy string match functions.
    /// </summary>
    public class FuzzyStringMatchContext : PoolableDbContext
    {
        /// <summary>
        ///     Represents a set of entities with <see cref="System.String" /> properties.
        /// </summary>
        public DbSet<FuzzyStringMatchTestEntity> FuzzyStringMatchTestEntities { get; set; }

        /// <summary>
        ///     Initializes a <see cref="FuzzyStringMatchContext" />.
        /// </summary>
        /// <param name="options">
        ///     The options to be used for configuration.
        /// </param>
        public FuzzyStringMatchContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("fuzzystrmatch");

            base.OnModelCreating(modelBuilder);
        }

        public static void Seed(FuzzyStringMatchContext context)
        {
            for (var i = 1; i <= 9; i++)
            {
                var text = "Some text " + i;
                context.FuzzyStringMatchTestEntities.Add(
                    new FuzzyStringMatchTestEntity { Id = i, Text = text });
            }

            context.SaveChanges();
        }
    }

    #endregion

    #region Helpers

    protected FuzzyStringMatchContext CreateContext()
        => Fixture.CreateContext();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    /// <summary>
    ///     Asserts that the SQL fragment appears in the logs.
    /// </summary>
    /// <param name="sql">The SQL statement or fragment to search for in the logs.</param>
    private void AssertContainsSql(string sql)
        => Assert.Contains(sql, Fixture.TestSqlLoggerFactory.Sql);

    #endregion
}
