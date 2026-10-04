using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.Query;

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
    [MinimumPostgresVersion(16, 0)]
    public async Task FuzzyStringMatchDaitchMokotoff()
    {
        await using var context = CreateContext();
        var results = await context.FuzzyStringMatchTestEntities
            .Select(x => EF.Functions.FuzzyStringMatchDaitchMokotoff(x.Text))
            .ToArrayAsync();

        Assert.Equal(9, results.Length);
        Assert.All(results, result => Assert.Equal(new[] { "463543" }, result));

        AssertSql(
            """
SELECT daitch_mokotoff(f."Text")
FROM "FuzzyStringMatchTestEntities" AS f
""");
    }

    [Theory]
    [InlineData("George", new[] { "595000" })]
    [InlineData("John", new[] { "160000", "460000" })]
    [InlineData("", null)]
    [InlineData(null, null)]
    [MinimumPostgresVersion(16, 0)]
    public async Task FuzzyStringMatchDaitchMokotoff_parameter(string? text, string[]? expected)
    {
        await using var context = CreateContext();
        var results = await context.FuzzyStringMatchTestEntities
            .Select(x => EF.Functions.FuzzyStringMatchDaitchMokotoff(text!))
            .ToArrayAsync();

        Assert.Equal(9, results.Length);
        Assert.All(results, result => Assert.Equal(expected, result));

        AssertContainsSql(text is null ? "daitch_mokotoff(NULL)" : "daitch_mokotoff(@text)");
    }

    [Fact]
    [MinimumPostgresVersion(16, 0)]
    public async Task FuzzyStringMatchDaitchMokotoff_null_result()
    {
        await using var context = CreateContext();
        var count = await context.FuzzyStringMatchTestEntities
            .CountAsync(x => EF.Functions.FuzzyStringMatchDaitchMokotoff(x.Text.Substring(0, 0)) == null);

        Assert.Equal(9, count);

        AssertSql(
            """
SELECT count(*)::int
FROM "FuzzyStringMatchTestEntities" AS f
WHERE daitch_mokotoff(substring(f."Text", 1, 0)) IS NULL
""");
    }

    [Fact]
    public void FuzzyStringMatchSoundex()
    {
        using var context = CreateContext();
        var _ = context.FuzzyStringMatchTestEntities
            .Select(x => EF.Functions.FuzzyStringMatchSoundex(x.Text))
            .ToArray();

        AssertContainsSql("""soundex(f."Text")""");
    }

    [Fact]
    public void FuzzyStringMatchDifference()
    {
        using var context = CreateContext();
        var _ = context.FuzzyStringMatchTestEntities
            .Select(x => EF.Functions.FuzzyStringMatchDifference(x.Text, "target"))
            .ToArray();

        AssertContainsSql("""difference(f."Text", 'target')""");
    }

    [Fact]
    public void FuzzyStringMatchLevenshtein()
    {
        using var context = CreateContext();
        var _ = context.FuzzyStringMatchTestEntities
            .Select(x => EF.Functions.FuzzyStringMatchLevenshtein(x.Text, "target"))
            .ToArray();

        AssertContainsSql("""levenshtein(f."Text", 'target')""");
    }

    [Fact]
    public void FuzzyStringMatchLevenshtein_With_Costs()
    {
        using var context = CreateContext();
        var _ = context.FuzzyStringMatchTestEntities
            .Select(x => EF.Functions.FuzzyStringMatchLevenshtein(x.Text, "target", 1, 2, 3))
            .ToArray();

        AssertContainsSql("""levenshtein(f."Text", 'target', 1, 2, 3)""");
    }

    [Fact]
    public void FuzzyStringMatchLevenshteinLessEqual()
    {
        using var context = CreateContext();
        var _ = context.FuzzyStringMatchTestEntities
            .Select(x => EF.Functions.FuzzyStringMatchLevenshteinLessEqual(x.Text, "target", 5))
            .ToArray();

        AssertContainsSql("""levenshtein_less_equal(f."Text", 'target', 5)""");
    }

    [Fact]
    public void FuzzyStringMatchLevenshteinLessEqual_With_Costs()
    {
        using var context = CreateContext();
        var _ = context.FuzzyStringMatchTestEntities
            .Select(x => EF.Functions.FuzzyStringMatchLevenshteinLessEqual(x.Text, "target", 1, 2, 3, 5))
            .ToArray();

        AssertContainsSql("""levenshtein_less_equal(f."Text", 'target', 1, 2, 3, 5)""");
    }

    [Fact]
    public void FuzzyStringMatchMetaphone()
    {
        using var context = CreateContext();
        var _ = context.FuzzyStringMatchTestEntities
            .Select(x => EF.Functions.FuzzyStringMatchMetaphone(x.Text, 6))
            .ToArray();

        AssertContainsSql("""metaphone(f."Text", 6)""");
    }

    [Fact]
    public void FuzzyStringMatchDoubleMetaphone()
    {
        using var context = CreateContext();
        var _ = context.FuzzyStringMatchTestEntities
            .Select(x => EF.Functions.FuzzyStringMatchDoubleMetaphone(x.Text))
            .ToArray();

        AssertContainsSql("""dmetaphone(f."Text")""");
    }

    [Fact]
    public void FuzzyStringMatchDoubleMetaphoneAlt()
    {
        using var context = CreateContext();
        var _ = context.FuzzyStringMatchTestEntities
            .Select(x => EF.Functions.FuzzyStringMatchDoubleMetaphoneAlt(x.Text))
            .ToArray();

        AssertContainsSql("""dmetaphone_alt(f."Text")""");
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

        protected override Task SeedAsync(FuzzyStringMatchContext context)
            => FuzzyStringMatchContext.SeedAsync(context);
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
        public string Text { get; set; } = null!;
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

        public static async Task SeedAsync(FuzzyStringMatchContext context)
        {
            for (var i = 1; i <= 9; i++)
            {
                var text = "Some text " + i;
                context.FuzzyStringMatchTestEntities.Add(
                    new FuzzyStringMatchTestEntity { Id = i, Text = text });
            }

            await context.SaveChangesAsync();
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
