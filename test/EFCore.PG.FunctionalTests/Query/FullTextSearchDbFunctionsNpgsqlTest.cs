using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

#pragma warning disable CS0618 // NpgsqlTsVector.Parse is obsolete

public class FullTextSearchDbFunctionsNpgsqlTest : IClassFixture<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
{
    protected NorthwindQueryNpgsqlFixture<NoopModelCustomizer> Fixture { get; }

    // ReSharper disable once UnusedParameter.Local
    public FullTextSearchDbFunctionsNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
    {
        Fixture = fixture;
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [Fact]
    public void TsVectorParse_converted_to_cast()
    {
        using var context = CreateContext();
        var tsvector = context.Customers.Select(c => NpgsqlTsVector.Parse("a b")).First();

        Assert.NotNull(tsvector);
        AssertSql(
            """
SELECT 'a b'::tsvector
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void ArrayToTsVector_constants()
    {
        using var context = CreateContext();
        var tsvector = context.Customers.Select(c => EF.Functions.ArrayToTsVector(new[] { "b", "c", "d" }))
            .First();

        Assert.Equal(NpgsqlTsVector.Parse("b c d").ToString(), tsvector.ToString());
        AssertSql(
            """
SELECT array_to_tsvector(ARRAY['b','c','d']::text[])
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void ArrayToTsVector_columns()
    {
        using var context = CreateContext();
        var tsvector = context.Customers
            .OrderBy(c => c.CustomerID)
            .Select(c => EF.Functions.ArrayToTsVector(new[] { c.CompanyName, c.Address }))
            .First();

        Assert.Equal(NpgsqlTsVector.Parse("'Alfreds Futterkiste' 'Obere Str. 57'").ToString(), tsvector.ToString());
        AssertSql(
            """
SELECT array_to_tsvector(ARRAY[c."CompanyName",c."Address"]::character varying(60)[])
FROM "Customers" AS c
ORDER BY c."CustomerID" NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void ToTsVector()
    {
        using var context = CreateContext();
        var tsvector = context.Customers.Select(c => EF.Functions.ToTsVector(c.CompanyName)).First();

        Assert.NotNull(tsvector);
        AssertSql(
            """
SELECT to_tsvector(c."CompanyName")
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void ToTsVector_with_constant_config()
    {
        using var context = CreateContext();
        var tsvector = context.Customers.Select(c => EF.Functions.ToTsVector("english", c.CompanyName)).First();

        Assert.NotNull(tsvector);
        AssertSql(
            """
SELECT to_tsvector('english', c."CompanyName")
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void ToTsVector_with_parameter_config()
    {
        using var context = CreateContext();
        var config = "english";
        var tsvector = context.Customers.Select(c => EF.Functions.ToTsVector(config, c.CompanyName)).First();

        Assert.NotNull(tsvector);
        AssertSql(
            """
SELECT to_tsvector('english', c."CompanyName")
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void TsQueryParse_converted_to_cast()
    {
        using var context = CreateContext();
        var tsquery = context.Customers.Select(c => NpgsqlTsQuery.Parse("a & b")).First();

        Assert.NotNull(tsquery);
        AssertSql(
            """
SELECT 'a & b'::tsquery
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void PlainToTsQuery()
    {
        using var context = CreateContext();
        var tsquery = context.Customers.Select(c => EF.Functions.PlainToTsQuery("a")).First();

        Assert.NotNull(tsquery);
        AssertSql(
            """
SELECT plainto_tsquery('a')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void PlainToTsQuery_with_constant_config()
    {
        using var context = CreateContext();
        var tsquery = context.Customers.Select(c => EF.Functions.PlainToTsQuery("english", "a")).First();

        Assert.NotNull(tsquery);
        AssertSql(
            """
SELECT plainto_tsquery('english', 'a')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void PlainToTsQuery_with_parameter_config()
    {
        using var context = CreateContext();
        var config = "english";
        var tsquery = context.Customers.Select(c => EF.Functions.PlainToTsQuery(config, "a")).First();

        Assert.NotNull(tsquery);
        AssertSql(
            """
SELECT plainto_tsquery('english', 'a')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void PhraseToTsQuery()
    {
        using var context = CreateContext();
        var tsquery = context.Customers.Select(c => EF.Functions.PhraseToTsQuery("a b")).First();

        Assert.NotNull(tsquery);
        AssertSql(
            """
SELECT phraseto_tsquery('a b')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void PhraseToTsQuery_with_constant_config()
    {
        using var context = CreateContext();
        var tsquery = context.Customers.Select(c => EF.Functions.PhraseToTsQuery("english", "a b")).First();

        Assert.NotNull(tsquery);
        AssertSql(
            """
SELECT phraseto_tsquery('english', 'a b')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void PhraseToTsQuery_with_parameter_config()
    {
        using var context = CreateContext();
        var config = "english";
        var tsquery = context.Customers.Select(c => EF.Functions.PhraseToTsQuery(config, "a b")).First();

        Assert.NotNull(tsquery);
        AssertSql(
            """
SELECT phraseto_tsquery('english', 'a b')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void ToTsQuery()
    {
        using var context = CreateContext();
        var tsquery = context.Customers.Select(c => EF.Functions.ToTsQuery("a & b")).First();

        Assert.NotNull(tsquery);
        AssertSql(
            """
SELECT to_tsquery('a & b')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void ToTsQuery_with_constant_config()
    {
        using var context = CreateContext();
        var tsquery = context.Customers.Select(c => EF.Functions.ToTsQuery("english", "a & b")).First();

        Assert.NotNull(tsquery);
        AssertSql(
            """
SELECT to_tsquery('english', 'a & b')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void ToTsQuery_with_parameter_config()
    {
        using var context = CreateContext();
        var config = "english";
        var tsquery = context.Customers.Select(c => EF.Functions.ToTsQuery(config, "a & b")).First();

        Assert.NotNull(tsquery);
        AssertSql(
            """
SELECT to_tsquery('english', 'a & b')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [ConditionalFact]
    [MinimumPostgresVersion(11, 0)]
    public void WebSearchToTsQuery()
    {
        using var context = CreateContext();
        var tsquery = context.Customers.Select(c => EF.Functions.WebSearchToTsQuery("a OR b")).First();

        Assert.NotNull(tsquery);
        AssertSql(
            """
SELECT websearch_to_tsquery('a OR b')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [ConditionalFact]
    [MinimumPostgresVersion(11, 0)]
    public void WebSearchToTsQuery_with_constant_config()
    {
        using var context = CreateContext();
        var tsquery = context.Customers.Select(c => EF.Functions.WebSearchToTsQuery("english", "a OR b")).First();

        Assert.NotNull(tsquery);
        AssertSql(
            """
SELECT websearch_to_tsquery('english', 'a OR b')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [ConditionalFact]
    [MinimumPostgresVersion(11, 0)]
    public void WebSearchToTsQuery_with_parameter_config()
    {
        using var context = CreateContext();
        var config = "english";
        var tsquery = context.Customers.Select(c => EF.Functions.WebSearchToTsQuery(config, "a OR b")).First();

        Assert.NotNull(tsquery);
        AssertSql(
            """
SELECT websearch_to_tsquery('english', 'a OR b')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void TsQuery_and()
    {
        using var context = CreateContext();
        var tsquery = context.Customers
            .Select(c => EF.Functions.ToTsQuery("a & b").And(EF.Functions.ToTsQuery("c & d")))
            .First();

        Assert.NotNull(tsquery);
        AssertSql(
            """
SELECT to_tsquery('a & b') && to_tsquery('c & d')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void TsQuery_or()
    {
        using var context = CreateContext();
        var tsquery = context.Customers
            .Select(c => EF.Functions.ToTsQuery("a & b").Or(EF.Functions.ToTsQuery("c & d")))
            .First();

        Assert.NotNull(tsquery);
        AssertSql(
            """
SELECT to_tsquery('a & b') || to_tsquery('c & d')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void TsQuery_ToNegative()
    {
        using var context = CreateContext();
        var tsquery = context.Customers
            .Select(c => EF.Functions.ToTsQuery("a & b").ToNegative())
            .First();

        Assert.NotNull(tsquery);
        AssertSql(
            """
SELECT !!to_tsquery('a & b')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void TsQuery_Contains()
    {
        using var context = CreateContext();
        var result = context.Customers
            .Select(c => EF.Functions.ToTsQuery("a & b").Contains(EF.Functions.ToTsQuery("b")))
            .First();

        Assert.True(result);
        AssertSql(
            """
SELECT to_tsquery('a & b') @> to_tsquery('b')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void TsQuery_IsContainedIn()
    {
        using var context = CreateContext();
        var result = context.Customers
            .Select(c => EF.Functions.ToTsQuery("b").IsContainedIn(EF.Functions.ToTsQuery("a & b")))
            .First();

        Assert.True(result);
        AssertSql(
            """
SELECT to_tsquery('b') <@ to_tsquery('a & b')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void GetNodeCount()
    {
        using var context = CreateContext();
        var nodeCount = context.Customers
            .Select(c => EF.Functions.ToTsQuery("b").GetNodeCount())
            .First();

        Assert.Equal(1, nodeCount);
        AssertSql(
            """
SELECT numnode(to_tsquery('b'))
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void GetQueryTree()
    {
        using var context = CreateContext();
        var queryTree = context.Customers
            .Select(c => EF.Functions.ToTsQuery("b").GetQueryTree())
            .First();

        Assert.NotEmpty(queryTree);
        AssertSql(
            """
SELECT querytree(to_tsquery('b'))
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void GetResultHeadline()
    {
        using var context = CreateContext();
        var headline = context.Customers
            .Select(c => EF.Functions.ToTsQuery("b").GetResultHeadline("a b c"))
            .First();

        Assert.NotEmpty(headline);
        AssertSql(
            """
SELECT ts_headline('a b c', to_tsquery('b'))
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void GetResultHeadline_with_options()
    {
        using var context = CreateContext();
        var headline = context.Customers
            .Select(c => EF.Functions.ToTsQuery("b").GetResultHeadline("a b c", "MinWords=1, MaxWords=2"))
            .First();

        Assert.NotEmpty(headline);
        AssertSql(
            """
SELECT ts_headline('a b c', to_tsquery('b'), 'MinWords=1, MaxWords=2')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void GetResultHeadline_with_constant_config_and_options()
    {
        using var context = CreateContext();
        var headline = context.Customers
            .Select(
                c => EF.Functions.ToTsQuery("b").GetResultHeadline(
                    "english",
                    "a b c",
                    "MinWords=1, MaxWords=2"))
            .First();

        Assert.NotEmpty(headline);
        AssertSql(
            """
SELECT ts_headline('english', 'a b c', to_tsquery('b'), 'MinWords=1, MaxWords=2')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void GetResultHeadline_with_parameter_config_and_options()
    {
        using var context = CreateContext();
        var config = "english";
        var headline = context.Customers
            .Select(
                c => EF.Functions.ToTsQuery("b").GetResultHeadline(
                    config,
                    "a b c",
                    "MinWords=1, MaxWords=2"))
            .First();

        Assert.NotEmpty(headline);
        AssertSql(
            """
@__config_1='english'

SELECT ts_headline(@__config_1::regconfig, 'a b c', to_tsquery('b'), 'MinWords=1, MaxWords=2')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void Rewrite()
    {
        using var context = CreateContext();
        var rewritten = context.Customers
            .Select(
                c => EF.Functions.ToTsQuery("a & b").Rewrite(
                    EF.Functions.ToTsQuery("b"),
                    EF.Functions.ToTsQuery("c")))
            .First();

        Assert.NotNull(rewritten);
        AssertSql(
            """
SELECT ts_rewrite(to_tsquery('a & b'), to_tsquery('b'), to_tsquery('c'))
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void Rewrite_with_select()
    {
        using var context = CreateContext();
        var rewritten = context.Customers
            .Select(
                c => EF.Functions.ToTsQuery("a & b").Rewrite(
                    """SELECT 'a'::tsquery, 'c'::tsquery"""))
            .First();

        Assert.NotNull(rewritten);
        AssertSql(
            """
SELECT ts_rewrite(to_tsquery('a & b'), 'SELECT ''a''::tsquery, ''c''::tsquery')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void ToPhrase()
    {
        using var context = CreateContext();
        var tsquery = context.Customers
            .Select(c => EF.Functions.ToTsQuery("a").ToPhrase(EF.Functions.ToTsQuery("b")))
            .First();

        Assert.NotNull(tsquery);
        AssertSql(
            """
SELECT tsquery_phrase(to_tsquery('a'), to_tsquery('b'))
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void ToPhrase_with_distance()
    {
        using var context = CreateContext();
        var tsquery = context.Customers
            .Select(c => EF.Functions.ToTsQuery("a").ToPhrase(EF.Functions.ToTsQuery("b"), 10))
            .First();

        Assert.NotNull(tsquery);
        AssertSql(
            """
SELECT tsquery_phrase(to_tsquery('a'), to_tsquery('b'), 10)
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void Matches_with_string()
    {
        using var context = CreateContext();
        var query = "b";
        var result = context.Customers
            .Select(c => EF.Functions.ToTsVector("a").Matches(query))
            .First();

        Assert.False(result);
        AssertSql(
            """
@__query_1='b'

SELECT to_tsvector('a') @@ plainto_tsquery(@__query_1)
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void Matches_with_TsQuery()
    {
        using var context = CreateContext();
        var result = context.Customers
            .Select(c => EF.Functions.ToTsVector("a").Matches(EF.Functions.ToTsQuery("b")))
            .First();

        Assert.False(result);
        AssertSql(
            """
SELECT to_tsvector('a') @@ to_tsquery('b')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void TsVector_Concat()
    {
        using var context = CreateContext();
        var tsVector = context.Customers
            .Select(c => EF.Functions.ToTsVector("b").Concat(EF.Functions.ToTsVector("c")))
            .First();

        Assert.Equal(NpgsqlTsVector.Parse("b:1 c:2").ToString(), tsVector.ToString());
        AssertSql(
            """
SELECT to_tsvector('b') || to_tsvector('c')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void SetWeight_with_enum()
    {
        using var context = CreateContext();
        var weightedTsVector = context.Customers
            .Select(c => EF.Functions.ToTsVector("a").SetWeight(NpgsqlTsVector.Lexeme.Weight.A))
            .First();

        Assert.NotNull(weightedTsVector);
        AssertSql(
            """
SELECT setweight(to_tsvector('a'), 'A')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void SetWeight_with_enum_and_lexemes()
    {
        using var context = CreateContext();
        var weightedTsVector = context.Customers
            .Select(c => EF.Functions.ToTsVector("a").SetWeight(NpgsqlTsVector.Lexeme.Weight.A, new[] { "a" }))
            .First();

        Assert.NotNull(weightedTsVector);
        AssertSql(
            """
SELECT setweight(to_tsvector('a'), 'A', ARRAY['a']::text[])
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void SetWeight_with_char()
    {
        using var context = CreateContext();
        var weightedTsVector = context.Customers
            .Select(c => EF.Functions.ToTsVector("a").SetWeight('A'))
            .First();

        Assert.NotNull(weightedTsVector);
        AssertSql(
            """
SELECT setweight(to_tsvector('a'), 'A')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void SetWeight_with_char_and_lexemes()
    {
        using var context = CreateContext();
        var weightedTsVector = context.Customers
            .Select(c => EF.Functions.ToTsVector("a").SetWeight('A', new[] { "a" }))
            .First();

        Assert.NotNull(weightedTsVector);
        AssertSql(
            """
SELECT setweight(to_tsvector('a'), 'A', ARRAY['a']::text[])
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void Delete_with_single_lexeme()
    {
        using var context = CreateContext();
        var tsVector = context.Customers
            .Select(c => EF.Functions.ToTsVector("b c").Delete("c"))
            .First();

        Assert.Equal(NpgsqlTsVector.Parse("b:1").ToString(), tsVector.ToString());
        AssertSql(
            """
SELECT ts_delete(to_tsvector('b c'), 'c')
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void Delete_with_multiple_lexemes()
    {
        using var context = CreateContext();
        var tsVector = context.Customers
            .Select(c => EF.Functions.ToTsVector("b c d").Delete(new[] { "c", "d" }))
            .First();

        Assert.Equal(NpgsqlTsVector.Parse("b:1").ToString(), tsVector.ToString());
        AssertSql(
            """
SELECT ts_delete(to_tsvector('b c d'), ARRAY['c','d']::text[])
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact(Skip = "Need to reimplement with \"char\"[]")]
    public void Filter()
    {
        using var context = CreateContext();
        var tsVector = context.Customers
            .Select(c => NpgsqlTsVector.Parse("b:1A c:2B d:3C").Filter(new[] { 'B', 'C' }))
            .First();

        Assert.Equal(NpgsqlTsVector.Parse("c:2B d:3C").ToString(), tsVector.ToString());
        AssertSql(
            """
SELECT ts_filter(CAST('b:1A c:2B d:3C' AS tsvector), CAST(ARRAY['B','C']::character(1)[] AS "char"[]))
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void GetLength()
    {
        using var context = CreateContext();
        var length = context.Customers
            .Select(c => EF.Functions.ToTsVector("c").GetLength())
            .First();

        Assert.Equal(1, length);
        AssertSql(
            """
SELECT length(to_tsvector('c'))
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void ToStripped()
    {
        using var context = CreateContext();
        var strippedTsVector = context.Customers
            .Select(c => EF.Functions.ToTsVector("c:A").ToStripped())
            .First();

        Assert.Equal(NpgsqlTsVector.Parse("c").ToString(), strippedTsVector.ToString());
        AssertSql(
            """
SELECT strip(to_tsvector('c:A'))
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void Rank()
    {
        using var context = CreateContext();
        var rank = context.Customers
            .Select(c => EF.Functions.ToTsVector("a b c").Rank(EF.Functions.ToTsQuery("b")))
            .First();

        Assert.True(rank > 0);
        AssertSql(
            """
SELECT ts_rank(to_tsvector('a b c'), to_tsquery('b'))
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void Rank_with_normalization()
    {
        using var context = CreateContext();
        var rank = context.Customers
            .Select(
                c => EF.Functions.ToTsVector("a b c").Rank(
                    EF.Functions.ToTsQuery("b"),
                    NpgsqlTsRankingNormalization.DivideByLength))
            .First();

        Assert.True(rank > 0);
        AssertSql(
            """
SELECT ts_rank(to_tsvector('a b c'), to_tsquery('b'), 2)
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void Rank_with_weights()
    {
        using var context = CreateContext();
        var rank = context.Customers
            .Select(
                c => EF.Functions.ToTsVector("a b c").Rank(
                    new float[] { 1, 1, 1, 1 },
                    EF.Functions.ToTsQuery("b")))
            .First();

        Assert.True(rank > 0);
        AssertSql(
            """
SELECT ts_rank(ARRAY[1,1,1,1]::real[], to_tsvector('a b c'), to_tsquery('b'))
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void Rank_with_weights_and_normalization()
    {
        using var context = CreateContext();
        var rank = context.Customers
            .Select(
                c => EF.Functions.ToTsVector("a b c").Rank(
                    new float[] { 1, 1, 1, 1 },
                    EF.Functions.ToTsQuery("b"),
                    NpgsqlTsRankingNormalization.DivideByLength))
            .First();

        Assert.True(rank > 0);
        AssertSql(
            """
SELECT ts_rank(ARRAY[1,1,1,1]::real[], to_tsvector('a b c'), to_tsquery('b'), 2)
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void RankCoverDensity()
    {
        using var context = CreateContext();
        var rank = context.Customers
            .Select(c => EF.Functions.ToTsVector("a b c").RankCoverDensity(EF.Functions.ToTsQuery("b")))
            .First();

        Assert.True(rank > 0);
        AssertSql(
            """
SELECT ts_rank_cd(to_tsvector('a b c'), to_tsquery('b'))
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void RankCoverDensity_with_normalization()
    {
        using var context = CreateContext();
        var rank = context.Customers
            .Select(
                c => EF.Functions.ToTsVector("a b c").RankCoverDensity(
                    EF.Functions.ToTsQuery("b"),
                    NpgsqlTsRankingNormalization.DivideByLength))
            .First();

        Assert.True(rank > 0);
        AssertSql(
            """
SELECT ts_rank_cd(to_tsvector('a b c'), to_tsquery('b'), 2)
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void RankCoverDensity_with_weights()
    {
        using var context = CreateContext();
        var rank = context.Customers
            .Select(
                c => EF.Functions.ToTsVector("a b c").RankCoverDensity(
                    new float[] { 1, 1, 1, 1 },
                    EF.Functions.ToTsQuery("b")))
            .First();

        Assert.True(rank > 0);
        AssertSql(
            """
SELECT ts_rank_cd(ARRAY[1,1,1,1]::real[], to_tsvector('a b c'), to_tsquery('b'))
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void RankCoverDensity_with_weights_and_normalization()
    {
        using var context = CreateContext();
        var rank = context.Customers
            .Select(
                c => EF.Functions.ToTsVector("a b c").RankCoverDensity(
                    new float[] { 1, 1, 1, 1 },
                    EF.Functions.ToTsQuery("b"),
                    NpgsqlTsRankingNormalization.DivideByLength))
            .First();

        Assert.True(rank > 0);
        AssertSql(
            """
SELECT ts_rank_cd(ARRAY[1,1,1,1]::real[], to_tsvector('a b c'), to_tsquery('b'), 2)
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void Basic_where()
    {
        using var context = CreateContext();
        var count = context.Customers
            .Count(c => EF.Functions.ToTsVector(c.ContactTitle).Matches(EF.Functions.ToTsQuery("owner")));

        Assert.True(count > 0);
    }

    [Fact]
    public void Complex_query()
    {
        using var context = CreateContext();
        var headline = context.Customers
            .Where(
                c => EF.Functions.ToTsVector(c.ContactTitle)
                    .SetWeight(NpgsqlTsVector.Lexeme.Weight.A)
                    .Matches(EF.Functions.ToTsQuery("accounting").ToPhrase(EF.Functions.ToTsQuery("manager"))))
            .Select(
                c => EF.Functions.ToTsQuery("accounting").ToPhrase(EF.Functions.ToTsQuery("manager"))
                    .GetResultHeadline(c.ContactTitle))
            .First();

        Assert.Equal("<b>Accounting</b> <b>Manager</b>", headline);
    }

    [Fact]
    public void Unaccent()
    {
        using var context = CreateContext();
        _ = context.Customers
            .Select(x => EF.Functions.Unaccent(x.ContactName))
            .FirstOrDefault();

        AssertSql(
            """
SELECT unaccent(c."ContactName")
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void Unaccent_with_constant_regdictionary()
    {
        using var context = CreateContext();
        _ = context.Customers
            .Select(x => EF.Functions.Unaccent("unaccent", x.ContactName))
            .FirstOrDefault();

        AssertSql(
            """
SELECT unaccent('unaccent', c."ContactName")
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact]
    public void Unaccent_with_parameter_regdictionary()
    {
        using var context = CreateContext();
        var regDictionary = "unaccent";
        _ = context.Customers
            .Select(x => EF.Functions.Unaccent(regDictionary, x.ContactName))
            .FirstOrDefault();

        AssertSql(
            """
@__regDictionary_1='unaccent'

SELECT unaccent(@__regDictionary_1::regdictionary, c."ContactName")
FROM "Customers" AS c
LIMIT 1
""");
    }

    [Fact] // #1652
    public void Match_and_boolean_operator_precedence()
    {
        using var context = CreateContext();
        _ = context.Customers
            .Count(
                c => EF.Functions.ToTsVector(c.ContactTitle)
                    .Matches(EF.Functions.ToTsQuery("owner").Or(EF.Functions.ToTsQuery("foo"))));

        AssertSql(
            """
SELECT count(*)::int
FROM "Customers" AS c
WHERE to_tsvector(c."ContactTitle") @@ (to_tsquery('owner') || to_tsquery('foo'))
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected NorthwindContext CreateContext()
        => Fixture.CreateContext();
}
