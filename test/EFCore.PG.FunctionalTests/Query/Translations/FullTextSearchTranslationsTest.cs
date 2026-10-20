namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class FullTextSearchTranslationsTest : IClassFixture<BasicTypesQueryNpgsqlFixture>
{
    private BasicTypesQueryNpgsqlFixture Fixture { get; }

    public FullTextSearchTranslationsTest(BasicTypesQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Matches_with_derived_tsquery_parameter(bool async)
    {
        using var context = Fixture.CreateContext();
        var tsQuery = new NpgsqlTsQueryLexeme("firs", NpgsqlTsQueryLexeme.Weight.None, true);
        var query = context.BasicTypesEntities
            .Select(b => EF.Functions.ToTsVector("simple", "first").Matches(tsQuery));

        var results = async ? await query.ToArrayAsync() : query.ToArray();

        Assert.NotEmpty(results);
        Assert.All(results, Assert.True);
        Fixture.TestSqlLoggerFactory.AssertBaseline(
            [
                """
@tsQuery=''firs':*' (DbType = Object)

SELECT to_tsvector('simple', 'first') @@ @tsQuery
FROM "BasicTypesEntities" AS b
"""
            ]);
    }
}
