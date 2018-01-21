using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class SimpleQueryNpgsqlTest : SimpleQueryTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public SimpleQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        [Fact]
        public void String_IndexOf_String()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CompanyName.IndexOf("ar") > 5),
                entryCount: 13);
            AssertContainsSql("WHERE (STRPOS(\"c\".\"CompanyName\", 'ar') - 1) > 5");
        }

        [Fact]
        public void String_IndexOf_not_found()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CompanyName.IndexOf("[") == -1),
                entryCount: 91);
            AssertContainsSql("WHERE (STRPOS(\"c\".\"CompanyName\", '[') - 1) = -1");
        }

        [Fact]
        public void String_IndexOf_Char()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.CompanyName.IndexOf('A') > 5),
                entryCount: 9);
            AssertContainsSql("WHERE (STRPOS(\"c\".\"CompanyName\", 'A') - 1) > 5");
        }

        private void AssertSql(params string[] expected)
            => AssertBaselineHack(expected);

        private void AssertContainsSql(params string[] expected)
            => AssertBaselineHack(expected, assertOrder: false);

        public void AssertBaselineHack(string[] expected, bool assertOrder = true)
        {
            var sqlStatements = Fixture.TestSqlLoggerFactory.SqlStatements;

            try
            {
                if (assertOrder)
                {
                    for (var i = 0; i < expected.Length; i++)
                    {
                        Assert.Equal(expected[i], sqlStatements[i], ignoreLineEndingDifferences: true);
                    }
                }
                else
                {
                    var contains = false;
                    foreach (var expectedFragment in expected)
                    {
                        var normalizedExpectedFragment = expectedFragment.Replace("\r", string.Empty).Replace("\n", string.Empty);

                        foreach (var statement in sqlStatements)
                        {
                            if (statement.Contains(normalizedExpectedFragment))
                            {
                                Assert.Contains(
                                   normalizedExpectedFragment,
                                   statement,
                                   StringComparison.OrdinalIgnoreCase);

                                contains = true;
                                break;
                            }
                        }
                        if(!contains)
                        {
                            Assert.Contains(
                               normalizedExpectedFragment,
                               sqlStatements);
                        }

                    }
                }
            }
            catch
            {
                throw;
            }
        }

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
