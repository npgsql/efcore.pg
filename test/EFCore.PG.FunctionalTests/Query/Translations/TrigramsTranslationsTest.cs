using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

/// <summary>
///     Provides unit tests for the pg_trgm module operator and function translations.
/// </summary>
/// <remarks>
///     See: https://www.postgresql.org/docs/current/pgtrgm.html
/// </remarks>
public class TrigramsTranslationsTest : IClassFixture<TrigramsTranslationsTest.TrigramsQueryNpgsqlFixture>
{
    private TrigramsQueryNpgsqlFixture Fixture { get; }

    // ReSharper disable once UnusedParameter.Local
    public TrigramsTranslationsTest(TrigramsQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [Fact]
    public void TrigramsShow()
    {
        using var context = CreateContext();
        _ = context.TrigramsTestEntities
            .Select(x => EF.Functions.TrigramsShow(x.Text))
            .ToArray();

        AssertContainsSql("""show_trgm(t."Text")""");
    }

    [Fact]
    public void TrigramsSimilarity()
    {
        using var context = CreateContext();
        _ = context.TrigramsTestEntities
            .Select(x => EF.Functions.TrigramsSimilarity(x.Text, "target"))
            .ToArray();

        AssertContainsSql("""similarity(t."Text", 'target')""");
    }

    [Fact]
    public void TrigramsWordSimilarity()
    {
        using var context = CreateContext();
        _ = context.TrigramsTestEntities
            .Select(x => EF.Functions.TrigramsWordSimilarity(x.Text, "target"))
            .ToArray();

        AssertContainsSql("""word_similarity(t."Text", 'target')""");
    }

    [ConditionalFact]
    [MinimumPostgresVersion(11, 0)]
    public void TrigramsStrictWordSimilarity()
    {
        using var context = CreateContext();
        _ = context.TrigramsTestEntities
            .Select(x => EF.Functions.TrigramsStrictWordSimilarity(x.Text, "target"))
            .ToArray();

        AssertContainsSql("""strict_word_similarity(t."Text", 'target')""");
    }

    [Fact]
    public void TrigramsAreSimilar()
    {
        using var context = CreateContext();
        _ = context.TrigramsTestEntities
            .Select(x => EF.Functions.TrigramsAreSimilar(x.Text, "target"))
            .ToArray();

        AssertContainsSql("""t."Text" % 'target'""");
    }

    [Fact]
    public void TrigramsAreWordSimilar()
    {
        using var context = CreateContext();
        _ = context.TrigramsTestEntities
            .Select(x => EF.Functions.TrigramsAreWordSimilar(x.Text, "target"))
            .ToArray();

        AssertContainsSql("""t."Text" <% 'target'""");
    }

    [Fact]
    public void TrigramsAreNotWordSimilar()
    {
        using var context = CreateContext();
        _ = context.TrigramsTestEntities
            .Select(x => EF.Functions.TrigramsAreNotWordSimilar(x.Text, "target"))
            .ToArray();

        AssertContainsSql("""t."Text" %> 'target'""");
    }

    [ConditionalFact]
    [MinimumPostgresVersion(11, 0)]
    public void TrigramsAreStrictWordSimilar()
    {
        using var context = CreateContext();
        _ = context.TrigramsTestEntities
            .Select(x => EF.Functions.TrigramsAreStrictWordSimilar(x.Text, "target"))
            .ToArray();

        AssertContainsSql("""t."Text" <<% 'target'""");
    }

    [ConditionalFact]
    [MinimumPostgresVersion(11, 0)]
    public void TrigramsAreNotStrictWordSimilar()
    {
        using var context = CreateContext();
        _ = context.TrigramsTestEntities
            .Select(x => EF.Functions.TrigramsAreNotStrictWordSimilar(x.Text, "target"))
            .ToArray();

        AssertContainsSql("""t."Text" %>> 'target'""");
    }

    [Fact]
    public void TrigramsSimilarityDistance()
    {
        using var context = CreateContext();
        _ = context.TrigramsTestEntities
            .Select(x => EF.Functions.TrigramsSimilarityDistance(x.Text, "target"))
            .ToArray();

        AssertContainsSql("""t."Text" <-> 'target'""");
    }

    [Fact]
    public void TrigramsWordSimilarityDistance()
    {
        using var context = CreateContext();
        _ = context.TrigramsTestEntities
            .Select(x => EF.Functions.TrigramsWordSimilarityDistance(x.Text, "target"))
            .ToArray();

        AssertContainsSql("""t."Text" <<-> 'target'""");
    }

    [Fact]
    public void TrigramsWordSimilarityDistanceInverted()
    {
        using var context = CreateContext();
        _ = context.TrigramsTestEntities
            .Select(x => EF.Functions.TrigramsWordSimilarityDistanceInverted(x.Text, "target"))
            .ToArray();

        AssertContainsSql("""t."Text" <->> 'target'""");
    }

    [ConditionalFact]
    [MinimumPostgresVersion(11, 0)]
    public void TrigramsStrictWordSimilarityDistance()
    {
        using var context = CreateContext();
        _ = context.TrigramsTestEntities
            .Select(x => EF.Functions.TrigramsStrictWordSimilarityDistance(x.Text, "target"))
            .ToArray();

        AssertContainsSql("""t."Text" <<<-> 'target'""");
    }

    [ConditionalFact]
    [MinimumPostgresVersion(11, 0)]
    public void TrigramsStrictWordSimilarityDistanceInverted()
    {
        using var context = CreateContext();
        _ = context.TrigramsTestEntities
            .Select(x => EF.Functions.TrigramsStrictWordSimilarityDistanceInverted(x.Text, "target"))
            .ToArray();

        AssertContainsSql("""t."Text" <->>> 'target'""");
    }

    [Fact] // #1659
    public void Concatenation_operator_precedence()
    {
        using var context = CreateContext();
        _ = context.TrigramsTestEntities
            .Where(e => EF.Functions.TrigramsAreSimilar(e.Text + " " + e.Text, "query"))
            .ToArray();

        AssertSql(
            """
SELECT t."Id", t."Text"
FROM "TrigramsTestEntities" AS t
WHERE (t."Text" || ' ' || t."Text") % 'query'
""");
    }

    [Fact] // #3072
    public void PgUnknownBinary_operator_precedence()
    {
        using var context = CreateContext();
        _ = context.TrigramsTestEntities
            .Where(e => 1 - EF.Functions.TrigramsSimilarityDistance(e.Text, "query") > 8)
            .ToArray();

        AssertSql(
            """
SELECT t."Id", t."Text"
FROM "TrigramsTestEntities" AS t
WHERE 1 - (t."Text" <-> 'query') > 8
""");
    }

    #region Fixtures

    /// <summary>
    ///     Represents a fixture suitable for testing trigrams operators.
    /// </summary>
    public class TrigramsQueryNpgsqlFixture : SharedStoreFixtureBase<TrigramsContext>
    {
        protected override string StoreName
            => "TrigramsQueryTest";

        protected override ITestStoreFactory TestStoreFactory
            => NpgsqlTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override Task SeedAsync(TrigramsContext context)
            => TrigramsContext.SeedAsync(context);
    }

    /// <summary>
    ///     Represents an entity suitable for testing trigrams operators.
    /// </summary>
    public class TrigramsTestEntity
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
    ///     Represents a database suitable for testing trigrams operators.
    /// </summary>
    public class TrigramsContext : PoolableDbContext
    {
        /// <summary>
        ///     Represents a set of entities with <see cref="System.String" /> properties.
        /// </summary>
        public DbSet<TrigramsTestEntity> TrigramsTestEntities { get; set; }

        /// <summary>
        ///     Initializes a <see cref="TrigramsContext" />.
        /// </summary>
        /// <param name="options">
        ///     The options to be used for configuration.
        /// </param>
        public TrigramsContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("pg_trgm");

            base.OnModelCreating(modelBuilder);
        }

        public static async Task SeedAsync(TrigramsContext context)
        {
            for (var i = 1; i <= 9; i++)
            {
                var text = "Some text " + i;
                context.TrigramsTestEntities.Add(
                    new TrigramsTestEntity { Id = i, Text = text });
            }

            await context.SaveChangesAsync();
        }
    }

    #endregion

    #region Helpers

    protected TrigramsContext CreateContext()
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
