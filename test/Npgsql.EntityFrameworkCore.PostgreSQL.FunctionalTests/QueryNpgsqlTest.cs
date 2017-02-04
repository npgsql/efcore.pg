using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Xunit;

#if NETSTANDARDAPP1_5
using System.Threading;
#endif

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class QueryNpgsqlTest : QueryTestBase<NorthwindQueryNpgsqlFixture>
    {
        public QueryNpgsqlTest(NorthwindQueryNpgsqlFixture fixture)
            : base(fixture) { }

        [Fact(Skip = "https://github.com/aspnet/EntityFramework/issues/7512")]
        public override void OrderBy_skip_take_distinct() { }

        #region Inherited

        public override void String_Contains_Literal()
        {
            base.String_Contains_Literal();
            Assert.Contains("WHERE STRPOS(\"c\".\"ContactName\", 'M') > 0", Sql);
        }

        public override void String_StartsWith_Literal()
        {
            base.String_StartsWith_Literal();
            Assert.Contains("WHERE STRPOS(\"c\".\"ContactName\", 'M') = 1", Sql);
        }

        public override void String_EndsWith_Literal()
        {
            base.String_EndsWith_Literal();
            Assert.Contains("WHERE RIGHT(\"c\".\"ContactName\", LENGTH('b')) = 'b'", Sql);
        }

        public override void Trim_in_predicate()
        {
            base.TrimEnd_in_predicate();
            Assert.Contains("WHERE REGEXP_REPLACE(\"c\".\"ContactTitle\", '\\s*$', '') = 'Owner'", Sql);
        }

        public override void TrimStart_in_predicate()
        {
            base.TrimStart_in_predicate();
            Assert.Contains("WHERE REGEXP_REPLACE(\"c\".\"ContactTitle\", '^\\s*', '') = 'Owner'", Sql);
        }

        public override void TrimEnd_in_predicate()
        {
            base.TrimEnd_in_predicate();
            Assert.Contains("WHERE REGEXP_REPLACE(\"c\".\"ContactTitle\", '\\s*$', '') = 'Owner'", Sql);
        }

        public override void IsNullOrWhiteSpace_in_predicate()
        {
            base.IsNullOrWhiteSpace_in_predicate();
            Assert.Contains("WHERE \"c\".\"Region\" IS NULL OR \"c\".\"Region\" ~ '^\\s*$'", Sql);
        }

        [Fact]
        public void IsNullOrWhiteSpace_in_predicate_with_newline()
        {
            using (var context = CreateContext())
            {
                var query = context.Set<Customer>()
                    .FirstOrDefault(c => string.IsNullOrWhiteSpace("\n"));
                Assert.NotNull(query);
            }
        }

        public override void Query_expression_with_to_string_and_contains()
        {
            base.Query_expression_with_to_string_and_contains();
            Assert.Contains("STRPOS(CAST(\"o\".\"EmployeeID\" AS text), '10') > 0", Sql);
        }

        #endregion

        // From here on we test Npgsql-specific capabilities

        #region Regular Expressions

        [Fact]
        public void Regex_IsMatch()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => Regex.IsMatch(c.CompanyName, "^A")),
                entryCount: 4);
            Assert.Contains("WHERE \"c\".\"CompanyName\" ~ ('(?p)' || '^A')", Sql);
        }

        [Fact]
        public void Regex_IsMatchOptionsNone()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => Regex.IsMatch(c.CompanyName, "^A", RegexOptions.None)),
                entryCount: 4);
            Assert.Contains("WHERE \"c\".\"CompanyName\" ~ ('(?p)' || '^A')", Sql);
        }

        [Fact]
        public void Regex_IsMatchOptionsIgnoreCase()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => Regex.IsMatch(c.CompanyName, "^a", RegexOptions.IgnoreCase)),
                entryCount: 4);
            Assert.Contains("WHERE \"c\".\"CompanyName\" ~ ('(?ip)' || '^a')", Sql);
        }

        [Fact]
        public void Regex_IsMatchOptionsMultiline()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => Regex.IsMatch(c.CompanyName, "^A", RegexOptions.Multiline)),
                entryCount: 4);
            Assert.Contains("WHERE \"c\".\"CompanyName\" ~ ('(?n)' || '^A')", Sql);
        }

        [Fact]
        public void Regex_IsMatchOptionsSingleline()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => Regex.IsMatch(c.CompanyName, "^A", RegexOptions.Singleline)),
                entryCount: 4);
            Assert.Contains("WHERE \"c\".\"CompanyName\" ~ '^A'", Sql);
        }

        [Fact]
        public void Regex_IsMatchOptionsIgnorePatternWhitespace()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => Regex.IsMatch(c.CompanyName, "^ A", RegexOptions.IgnorePatternWhitespace)),
                entryCount: 4);
            Assert.Contains("WHERE \"c\".\"CompanyName\" ~ ('(?px)' || '^ A')", Sql);
        }

        [Fact]
        public void Regex_IsMatchOptionsUnsupported()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => Regex.IsMatch(c.CompanyName, "^A", RegexOptions.RightToLeft)),
                entryCount: 4);
            Assert.DoesNotContain("WHERE \"c\".\"CompanyName\" ~ ", Sql);
        }

        #endregion

        #region Trim

        [Fact]
        public void Trim_with_chars()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Trim('M', 's') == "aria Ander"),
                entryCount: 1);
            Assert.Contains("WHERE BTRIM(\"c\".\"ContactName\", 'Ms')", Sql);
        }

        [Fact]
        public void TrimStart_with_chars()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.TrimStart('M') == "aria Anders"),
                entryCount: 1);
            Assert.Contains("WHERE LTRIM(\"c\".\"ContactName\", 'M')", Sql);
        }

        [Fact]
        public void TrimEnd_with_chars()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.TrimEnd('s') == "Maria Ander"),
                entryCount: 1);
            Assert.Contains("WHERE RTRIM(\"c\".\"ContactName\", 's')", Sql);
        }

        #endregion

        const string FileLineEnding = @"
";
        protected override void ClearLog() => TestSqlLoggerFactory.Reset();

        static string Sql => TestSqlLoggerFactory.Sql.Replace(Environment.NewLine, FileLineEnding);
    }
}
