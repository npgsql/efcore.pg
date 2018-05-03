using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using NpgsqlTypes;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class FullTextSearchDbFunctionsNpgsqlTest : IClassFixture<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        protected NorthwindQueryNpgsqlFixture<NoopModelCustomizer> Fixture { get; }

        public FullTextSearchDbFunctionsNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        [Fact]
        public void TsVectorParse_converted_to_cast()
        {
            using (var context = CreateContext())
            {
                var tsvector = context.Customers.Select(c => NpgsqlTsVector.Parse("a b")).First();
                Assert.NotNull(tsvector);
            }

            AssertSql(
                @"SELECT CAST('a b' AS tsvector)
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void ArrayToTsVector()
        {
            using (var context = CreateContext())
            {
                var tsvector = context.Customers.Select(c => EF.Functions.ArrayToTsVector(new[] { "b", "c", "d" }))
                    .First();
                Assert.Equal(NpgsqlTsVector.Parse("b c d").ToString(), tsvector.ToString());
            }

            AssertSql(
                @"SELECT array_to_tsvector(ARRAY['b','c','d'])
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void ArrayToTsVector_From_Columns_Throws_NotSupportedException()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<NotSupportedException>(
                    () => context.Customers
                        .Select(c => EF.Functions.ArrayToTsVector(new[] { c.CompanyName, c.Address }))
                        .First());
            }
        }

        [Fact]
        public void ToTsVector()
        {
            using (var context = CreateContext())
            {
                var tsvector = context.Customers.Select(c => EF.Functions.ToTsVector(c.CompanyName)).First();
                Assert.NotNull(tsvector);
            }

            AssertSql(
                @"SELECT to_tsvector(c.""CompanyName"")
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void ToTsVector_With_Config()
        {
            using (var context = CreateContext())
            {
                var tsvector = context.Customers.Select(c => EF.Functions.ToTsVector("english", c.CompanyName)).First();
                Assert.NotNull(tsvector);
            }

            AssertSql(
                @"SELECT to_tsvector('english', c.""CompanyName"")
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void TsQueryParse_converted_to_cast()
        {
            using (var context = CreateContext())
            {
                var tsquery = context.Customers.Select(c => NpgsqlTsQuery.Parse("a & b")).First();
                Assert.NotNull(tsquery);
            }

            AssertSql(
                @"SELECT CAST('a & b' AS tsquery)
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void PlainToTsQuery()
        {
            using (var context = CreateContext())
            {
                var tsquery = context.Customers.Select(c => EF.Functions.PlainToTsQuery("a")).First();
                Assert.NotNull(tsquery);
            }

            AssertSql(
                @"SELECT plainto_tsquery('a')
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void PlainToTsQuery_With_Config()
        {
            using (var context = CreateContext())
            {
                var tsquery = context.Customers.Select(c => EF.Functions.PlainToTsQuery("english", "a")).First();
                Assert.NotNull(tsquery);
            }

            AssertSql(
                @"SELECT plainto_tsquery('english', 'a')
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void PhraseToTsQuery()
        {
            using (var context = CreateContext())
            {
                var tsquery = context.Customers.Select(c => EF.Functions.PhraseToTsQuery("a b")).First();
                Assert.NotNull(tsquery);
            }

            AssertSql(
                @"SELECT phraseto_tsquery('a b')
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void PhraseToTsQuery_With_Config()
        {
            using (var context = CreateContext())
            {
                var tsquery = context.Customers.Select(c => EF.Functions.PhraseToTsQuery("english", "a b")).First();
                Assert.NotNull(tsquery);
            }

            AssertSql(
                @"SELECT phraseto_tsquery('english', 'a b')
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void ToTsQuery()
        {
            using (var context = CreateContext())
            {
                var tsquery = context.Customers.Select(c => EF.Functions.ToTsQuery("a & b")).First();
                Assert.NotNull(tsquery);
            }

            AssertSql(
                @"SELECT to_tsquery('a & b')
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void ToTsQuery_With_Config()
        {
            using (var context = CreateContext())
            {
                var tsquery = context.Customers.Select(c => EF.Functions.ToTsQuery("english", "a & b")).First();
                Assert.NotNull(tsquery);
            }

            AssertSql(
                @"SELECT to_tsquery('english', 'a & b')
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void TsQueryAnd()
        {
            using (var context = CreateContext())
            {
                var tsquery = context.Customers
                    .Select(c => EF.Functions.ToTsQuery("a & b").And(EF.Functions.ToTsQuery("c & d")))
                    .First();
                Assert.NotNull(tsquery);
            }

            AssertSql(
                @"SELECT to_tsquery('a & b') && to_tsquery('c & d')
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void TsQueryOr()
        {
            using (var context = CreateContext())
            {
                var tsquery = context.Customers
                    .Select(c => EF.Functions.ToTsQuery("a & b").Or(EF.Functions.ToTsQuery("c & d")))
                    .First();
                Assert.NotNull(tsquery);
            }

            AssertSql(
                @"SELECT to_tsquery('a & b') || to_tsquery('c & d')
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void TsQueryToNegative()
        {
            using (var context = CreateContext())
            {
                var tsquery = context.Customers
                    .Select(c => EF.Functions.ToTsQuery("a & b").ToNegative())
                    .First();
                Assert.NotNull(tsquery);
            }

            AssertSql(
                @"SELECT !!to_tsquery('a & b')
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void TsQueryContains()
        {
            using (var context = CreateContext())
            {
                var result = context.Customers
                    .Select(c => EF.Functions.ToTsQuery("a & b").Contains(EF.Functions.ToTsQuery("b")))
                    .First();
                Assert.True(result);
            }

            AssertSql(
                @"SELECT to_tsquery('a & b') @> to_tsquery('b')
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void TsQueryIsContainedIn()
        {
            using (var context = CreateContext())
            {
                var result = context.Customers
                    .Select(c => EF.Functions.ToTsQuery("b").IsContainedIn(EF.Functions.ToTsQuery("a & b")))
                    .First();
                Assert.True(result);
            }

            AssertSql(
                @"SELECT to_tsquery('b') <@ to_tsquery('a & b')
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void GetNodeCount()
        {
            using (var context = CreateContext())
            {
                var nodeCount = context.Customers
                    .Select(c => EF.Functions.ToTsQuery("b").GetNodeCount())
                    .First();
                Assert.Equal(1, nodeCount);
            }

            AssertSql(
                @"SELECT numnode(to_tsquery('b'))
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void GetQueryTree()
        {
            using (var context = CreateContext())
            {
                var queryTree = context.Customers
                    .Select(c => EF.Functions.ToTsQuery("b").GetQueryTree())
                    .First();
                Assert.NotEmpty(queryTree);
            }

            AssertSql(
                @"SELECT querytree(to_tsquery('b'))
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void GetResultHeadline()
        {
            using (var context = CreateContext())
            {
                var headline = context.Customers
                    .Select(c => EF.Functions.ToTsQuery("b").GetResultHeadline("a b c"))
                    .First();
                Assert.NotEmpty(headline);
            }

            AssertSql(
                @"SELECT ts_headline('a b c', to_tsquery('b'))
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void GetResultHeadline_With_Options()
        {
            using (var context = CreateContext())
            {
                var headline = context.Customers
                    .Select(c => EF.Functions.ToTsQuery("b").GetResultHeadline("a b c", "MinWords=1, MaxWords=2"))
                    .First();
                Assert.NotEmpty(headline);
            }

            AssertSql(
                @"SELECT ts_headline('a b c', to_tsquery('b'), 'MinWords=1, MaxWords=2')
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void GetResultHeadline_With_Config_And_Options()
        {
            using (var context = CreateContext())
            {
                var headline = context.Customers
                    .Select(
                        c => EF.Functions.ToTsQuery("b").GetResultHeadline(
                            "english",
                            "a b c",
                            "MinWords=1, MaxWords=2"))
                    .First();
                Assert.NotEmpty(headline);
            }

            AssertSql(
                @"SELECT ts_headline('english', 'a b c', to_tsquery('b'), 'MinWords=1, MaxWords=2')
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void Rewrite()
        {
            using (var context = CreateContext())
            {
                var rewritten = context.Customers
                    .Select(
                        c => EF.Functions.ToTsQuery("a & b").Rewrite(
                            EF.Functions.ToTsQuery("b"),
                            EF.Functions.ToTsQuery("c")))
                    .First();
                Assert.NotNull(rewritten);
            }

            AssertSql(
                @"SELECT ts_rewrite(to_tsquery('a & b'), to_tsquery('b'), to_tsquery('c'))
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void ToPhrase()
        {
            using (var context = CreateContext())
            {
                var tsquery = context.Customers
                    .Select(c => EF.Functions.ToTsQuery("a").ToPhrase(EF.Functions.ToTsQuery("b")))
                    .First();
                Assert.NotNull(tsquery);
            }

            AssertSql(
                @"SELECT tsquery_phrase(to_tsquery('a'), to_tsquery('b'))
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void ToPhrase_With_Distance()
        {
            using (var context = CreateContext())
            {
                var tsquery = context.Customers
                    .Select(c => EF.Functions.ToTsQuery("a").ToPhrase(EF.Functions.ToTsQuery("b"), 10))
                    .First();
                Assert.NotNull(tsquery);
            }

            AssertSql(
                @"SELECT tsquery_phrase(to_tsquery('a'), to_tsquery('b'), 10)
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void Matches_With_String()
        {
            using (var context = CreateContext())
            {
                var result = context.Customers
                    .Select(c => EF.Functions.ToTsVector("a").Matches("b"))
                    .First();
                Assert.False(result);
            }

            AssertSql(
                @"SELECT to_tsvector('a') @@ 'b'
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void Matches_With_Tsquery()
        {
            using (var context = CreateContext())
            {
                var result = context.Customers
                    .Select(c => EF.Functions.ToTsVector("a").Matches(EF.Functions.ToTsQuery("b")))
                    .First();
                Assert.False(result);
            }

            AssertSql(
                @"SELECT to_tsvector('a') @@ to_tsquery('b')
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void TsVectorConcat()
        {
            using (var context = CreateContext())
            {
                var tsVector = context.Customers
                    .Select(c => EF.Functions.ToTsVector("b").Concat(EF.Functions.ToTsVector("c")))
                    .First();
                Assert.Equal(NpgsqlTsVector.Parse("b:1 c:2").ToString(), tsVector.ToString());
            }

            AssertSql(
                @"SELECT to_tsvector('b') || to_tsvector('c')
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void Setweight_With_Enum()
        {
            using (var context = CreateContext())
            {
                var weightedTsVector = context.Customers
                    .Select(c => EF.Functions.ToTsVector("a").SetWeight(NpgsqlTsVector.Lexeme.Weight.A))
                    .First();
                Assert.NotNull(weightedTsVector);
            }

            AssertSql(
                @"SELECT setweight(to_tsvector('a'), 'A')
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void Setweight_With_Enum_And_Lexemes()
        {
            using (var context = CreateContext())
            {
                var weightedTsVector = context.Customers
                    .Select(c => EF.Functions.ToTsVector("a").SetWeight(NpgsqlTsVector.Lexeme.Weight.A, new[] { "a" }))
                    .First();
                Assert.NotNull(weightedTsVector);
            }

            AssertSql(
                @"SELECT setweight(to_tsvector('a'), 'A', ARRAY['a'])
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void Setweight_With_Char()
        {
            using (var context = CreateContext())
            {
                var weightedTsVector = context.Customers
                    .Select(c => EF.Functions.ToTsVector("a").SetWeight('A'))
                    .First();
                Assert.NotNull(weightedTsVector);
            }

            AssertSql(
                @"SELECT setweight(to_tsvector('a'), 'A')
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void Setweight_With_Char_And_Lexemes()
        {
            using (var context = CreateContext())
            {
                var weightedTsVector = context.Customers
                    .Select(c => EF.Functions.ToTsVector("a").SetWeight('A', new[] { "a" }))
                    .First();
                Assert.NotNull(weightedTsVector);
            }

            AssertSql(
                @"SELECT setweight(to_tsvector('a'), 'A', ARRAY['a'])
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void Delete_With_Single_Lexeme()
        {
            using (var context = CreateContext())
            {
                var tsVector = context.Customers
                    .Select(c => EF.Functions.ToTsVector("b c").Delete("c"))
                    .First();
                Assert.Equal(NpgsqlTsVector.Parse("b:1").ToString(), tsVector.ToString());
            }

            AssertSql(
                @"SELECT ts_delete(to_tsvector('b c'), 'c')
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void Delete_With_Multiple_Lexemes()
        {
            using (var context = CreateContext())
            {
                var tsVector = context.Customers
                    .Select(c => EF.Functions.ToTsVector("b c d").Delete(new[] { "c", "d" }))
                    .First();
                Assert.Equal(NpgsqlTsVector.Parse("b:1").ToString(), tsVector.ToString());
            }

            AssertSql(
                @"SELECT ts_delete(to_tsvector('b c d'), ARRAY['c','d'])
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void Filter()
        {
            using (var context = CreateContext())
            {
                var tsVector = context.Customers
                    .Select(c => NpgsqlTsVector.Parse("b:1A c:2B d:3C").Filter(new[] { 'B', 'C' }))
                    .First();
                Assert.Equal(NpgsqlTsVector.Parse("c:2B d:3C").ToString(), tsVector.ToString());
            }

            AssertSql(
                @"SELECT ts_filter(CAST('b:1A c:2B d:3C' AS tsvector), CAST(ARRAY['B','C'] AS ""char""[]))
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void GetLength()
        {
            using (var context = CreateContext())
            {
                var length = context.Customers
                    .Select(c => EF.Functions.ToTsVector("c").GetLength())
                    .First();
                Assert.Equal(1, length);
            }

            AssertSql(
                @"SELECT length(to_tsvector('c'))
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void ToStripped()
        {
            using (var context = CreateContext())
            {
                var strippedTsVector = context.Customers
                    .Select(c => EF.Functions.ToTsVector("c:A").ToStripped())
                    .First();
                Assert.Equal(NpgsqlTsVector.Parse("c").ToString(), strippedTsVector.ToString());
            }

            AssertSql(
                @"SELECT strip(to_tsvector('c:A'))
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void Rank()
        {
            using (var context = CreateContext())
            {
                var rank = context.Customers
                    .Select(c => EF.Functions.ToTsVector("a b c").Rank(EF.Functions.ToTsQuery("b")))
                    .First();
                Assert.True(rank > 0);
            }

            AssertSql(
                @"SELECT ts_rank(to_tsvector('a b c'), to_tsquery('b'))
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void Rank_With_Normalization()
        {
            using (var context = CreateContext())
            {
                var rank = context.Customers
                    .Select(
                        c => EF.Functions.ToTsVector("a b c").Rank(
                            EF.Functions.ToTsQuery("b"),
                            NpgsqlTsRankingNormalization.DivideByLength))
                    .First();
                Assert.True(rank > 0);
            }

            AssertSql(
                @"SELECT ts_rank(to_tsvector('a b c'), to_tsquery('b'), 2)
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void Rank_With_Weights()
        {
            using (var context = CreateContext())
            {
                var rank = context.Customers
                    .Select(
                        c => EF.Functions.ToTsVector("a b c").Rank(
                            new float[] { 1, 1, 1, 1 },
                            EF.Functions.ToTsQuery("b")))
                    .First();
                Assert.True(rank > 0);
            }

            AssertSql(
                @"SELECT ts_rank(ARRAY[1,1,1,1], to_tsvector('a b c'), to_tsquery('b'))
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void Rank_With_Weights_And_Normalization()
        {
            using (var context = CreateContext())
            {
                var rank = context.Customers
                    .Select(
                        c => EF.Functions.ToTsVector("a b c").Rank(
                            new float[] { 1, 1, 1, 1 },
                            EF.Functions.ToTsQuery("b"),
                            NpgsqlTsRankingNormalization.DivideByLength))
                    .First();
                Assert.True(rank > 0);
            }

            AssertSql(
                @"SELECT ts_rank(ARRAY[1,1,1,1], to_tsvector('a b c'), to_tsquery('b'), 2)
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void RankCoverDensity()
        {
            using (var context = CreateContext())
            {
                var rank = context.Customers
                    .Select(c => EF.Functions.ToTsVector("a b c").RankCoverDensity(EF.Functions.ToTsQuery("b")))
                    .First();
                Assert.True(rank > 0);
            }

            AssertSql(
                @"SELECT ts_rank_cd(to_tsvector('a b c'), to_tsquery('b'))
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void RankCoverDensity_With_Normalization()
        {
            using (var context = CreateContext())
            {
                var rank = context.Customers
                    .Select(
                        c => EF.Functions.ToTsVector("a b c").RankCoverDensity(
                            EF.Functions.ToTsQuery("b"),
                            NpgsqlTsRankingNormalization.DivideByLength))
                    .First();
                Assert.True(rank > 0);
            }

            AssertSql(
                @"SELECT ts_rank_cd(to_tsvector('a b c'), to_tsquery('b'), 2)
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void RankCoverDensity_With_Weights()
        {
            using (var context = CreateContext())
            {
                var rank = context.Customers
                    .Select(
                        c => EF.Functions.ToTsVector("a b c").RankCoverDensity(
                            new float[] { 1, 1, 1, 1 },
                            EF.Functions.ToTsQuery("b")))
                    .First();
                Assert.True(rank > 0);
            }

            AssertSql(
                @"SELECT ts_rank_cd(ARRAY[1,1,1,1], to_tsvector('a b c'), to_tsquery('b'))
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void RankCoverDensity_With_Weights_And_Normalization()
        {
            using (var context = CreateContext())
            {
                var rank = context.Customers
                    .Select(
                        c => EF.Functions.ToTsVector("a b c").RankCoverDensity(
                            new float[] { 1, 1, 1, 1 },
                            EF.Functions.ToTsQuery("b"),
                            NpgsqlTsRankingNormalization.DivideByLength))
                    .First();
                Assert.True(rank > 0);
            }

            AssertSql(
                @"SELECT ts_rank_cd(ARRAY[1,1,1,1], to_tsvector('a b c'), to_tsquery('b'), 2)
FROM ""Customers"" AS c
LIMIT 1");
        }

        [Fact]
        public void Basic_where()
        {
            using (var context = CreateContext())
            {
                var count = context.Customers
                    .Count(c => EF.Functions.ToTsVector(c.ContactTitle).Matches(EF.Functions.ToTsQuery("owner")));
                Assert.True(count > 0);
            }
        }

        [Fact]
        public void Complex_query()
        {
            using (var context = CreateContext())
            {
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
        }

        void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected NorthwindContext CreateContext() => Fixture.CreateContext();
    }
}
