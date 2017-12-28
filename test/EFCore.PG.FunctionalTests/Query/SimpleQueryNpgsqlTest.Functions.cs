using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class SimpleQueryNpgsqlTest
    {
        public override void String_Contains_Literal()
        {
            base.String_Contains_Literal();
            AssertContainsSql("WHERE STRPOS(\"c\".\"ContactName\", 'M') > 0");
        }

        public override void String_StartsWith_Literal()
        {
            base.String_StartsWith_Literal();
            AssertContainsSql("WHERE \"c\".\"ContactName\" LIKE 'M%'");
        }

        [Fact]
        public void String_StartsWith_Literal_with_escaping()
        {
            AssertQuery<Customer>(cs => cs.Where(c => c.ContactName.StartsWith(@"_a%b\c")));
            AssertContainsSql(@"WHERE ""c"".""ContactName"" LIKE '\_a\%b\\c%'");
        }

        public override void String_StartsWith_Column()
        {
            AssertQuery<Customer>(cs => cs.Where(c => c.ContactName.StartsWith(c.City)));
            AssertContainsSql(@"WHERE ""c"".""ContactName"" LIKE (""c"".""City"" || '%') AND (LEFT(""c"".""ContactName"", LENGTH(""c"".""City"")) = ""c"".""City"")");
        }

        public override void String_EndsWith_Literal()
        {
            base.String_EndsWith_Literal();
            AssertContainsSql("WHERE RIGHT(\"c\".\"ContactName\", LENGTH('b')) = 'b'");
        }

        public override void Trim_without_argument_in_predicate()
        {
            base.Trim_without_argument_in_predicate();
            AssertContainsSql(@"WHERE REGEXP_REPLACE(""c"".""ContactTitle"", '^\s*(.*?)\s*$', '\1') = 'Owner'");
        }

        public override void Trim_with_char_argument_in_predicate()
        {
            base.Trim_with_char_argument_in_predicate();
            AssertContainsSql("WHERE BTRIM(\"c\".\"ContactTitle\", 'O')");
        }

        public override void Trim_with_char_array_argument_in_predicate()
        {
            base.Trim_with_char_array_argument_in_predicate();
            AssertContainsSql("WHERE BTRIM(\"c\".\"ContactTitle\", 'Or')");
        }

        public override void TrimStart_without_arguments_in_predicate()
        {
            base.TrimStart_without_arguments_in_predicate();
            AssertContainsSql("WHERE REGEXP_REPLACE(\"c\".\"ContactTitle\", '^\\s*', '') = 'Owner'");
        }

        public override void TrimStart_with_char_argument_in_predicate()
        {
            base.TrimStart_with_char_argument_in_predicate();
            AssertContainsSql("WHERE LTRIM(\"c\".\"ContactTitle\", 'O')");
        }

        public override void TrimStart_with_char_array_argument_in_predicate()
        {
            base.TrimStart_with_char_array_argument_in_predicate();
            AssertContainsSql("WHERE LTRIM(\"c\".\"ContactTitle\", 'Ow')");
        }

        public override void TrimEnd_without_arguments_in_predicate()
        {
            base.TrimEnd_without_arguments_in_predicate();
            AssertContainsSql("WHERE REGEXP_REPLACE(\"c\".\"ContactTitle\", '\\s*$', '') = 'Owner'");
        }

        public override void TrimEnd_with_char_argument_in_predicate()
        {
            base.TrimEnd_with_char_argument_in_predicate();
            AssertContainsSql("WHERE RTRIM(\"c\".\"ContactTitle\", 'r')");
        }

        public override void TrimEnd_with_char_array_argument_in_predicate()
        {
            base.TrimEnd_with_char_array_argument_in_predicate();
            AssertContainsSql("WHERE RTRIM(\"c\".\"ContactTitle\", 'er')");
        }

        public override void IsNullOrWhiteSpace_in_predicate()
        {
            base.IsNullOrWhiteSpace_in_predicate();
            AssertContainsSql("WHERE \"c\".\"Region\" IS NULL OR (\"c\".\"Region\" ~ '^\\s*$' = TRUE)");
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
            AssertContainsSql("STRPOS(CAST(\"o\".\"EmployeeID\" AS text), '10') > 0");
        }

        public override void Where_datetime_now()
        {
            base.Where_datetime_now();
            AssertContainsSql("WHERE NOW() <>");
        }

        public override void Where_datetime_utcnow()
        {
            base.Where_datetime_utcnow();
            AssertContainsSql("WHERE NOW() AT TIME ZONE 'UTC' <>");
        }

        public override void Where_datetime_date_component()
        {
            base.Where_datetime_date_component();
            AssertContainsSql("WHERE DATE_TRUNC('day', \"o\".\"OrderDate\")");
        }

        public override void Where_datetime_year_component()
        {
            base.Where_datetime_year_component();
            AssertContainsSql("DATE_PART('year', \"o\".\"OrderDate\")");
        }

        public override void Where_datetime_month_component()
        {
            base.Where_datetime_month_component();
            AssertContainsSql("DATE_PART('month', \"o\".\"OrderDate\")");
        }

        public override void Where_datetime_dayOfYear_component()
        {
            base.Where_datetime_dayOfYear_component();
            AssertContainsSql("DATE_PART('doy', \"o\".\"OrderDate\")");
        }

        public override void Where_datetime_day_component()
        {
            base.Where_datetime_day_component();
            AssertContainsSql("DATE_PART('day', \"o\".\"OrderDate\")");
        }

        public override void Where_datetime_hour_component()
        {
            base.Where_datetime_hour_component();
            AssertContainsSql("DATE_PART('hour', \"o\".\"OrderDate\")");
        }

        public override void Where_datetime_minute_component()
        {
            base.Where_datetime_minute_component();
            AssertContainsSql("DATE_PART('minute', \"o\".\"OrderDate\")");
        }

        public override void Where_datetime_second_component()
        {
            base.Where_datetime_second_component();
            AssertContainsSql("DATE_PART('second', \"o\".\"OrderDate\")");
        }

        // ReSharper disable once RedundantOverriddenMember
        public override void Where_datetime_millisecond_component()
        {
            // SQL translation not implemented, too annoying
            base.Where_datetime_millisecond_component();
        }

        [Fact]
        public void Where_datetime_dayOfWeek_component()
        {
            AssertQuery<Order>(
                oc => oc.Where(o =>
                        o.OrderDate.Value.DayOfWeek == DayOfWeek.Tuesday),
                entryCount: 168);
            AssertContainsSql("WHERE CAST(FLOOR(DATE_PART('dow', \"o\".\"OrderDate\")) AS int4)");
        } 

        [Fact]
        public void Regex_IsMatch()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => Regex.IsMatch(c.CompanyName, "^A")),
                entryCount: 4);
            AssertContainsSql("WHERE \"c\".\"CompanyName\" ~ ('(?p)' || '^A')");
        }

        [Fact]
        public void Regex_IsMatchOptionsNone()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => Regex.IsMatch(c.CompanyName, "^A", RegexOptions.None)),
                entryCount: 4);
            AssertContainsSql("WHERE \"c\".\"CompanyName\" ~ ('(?p)' || '^A')");
        }

        [Fact]
        public void Regex_IsMatchOptionsIgnoreCase()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => Regex.IsMatch(c.CompanyName, "^a", RegexOptions.IgnoreCase)),
                entryCount: 4);
            AssertContainsSql("WHERE \"c\".\"CompanyName\" ~ ('(?ip)' || '^a')");
        }

        [Fact]
        public void Regex_IsMatchOptionsMultiline()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => Regex.IsMatch(c.CompanyName, "^A", RegexOptions.Multiline)),
                entryCount: 4);
            AssertContainsSql("WHERE \"c\".\"CompanyName\" ~ ('(?n)' || '^A')");
        }

        [Fact]
        public void Regex_IsMatchOptionsSingleline()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => Regex.IsMatch(c.CompanyName, "^A", RegexOptions.Singleline)),
                entryCount: 4);
            AssertContainsSql("WHERE \"c\".\"CompanyName\" ~ '^A'");
        }

        [Fact]
        public void Regex_IsMatchOptionsIgnorePatternWhitespace()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => Regex.IsMatch(c.CompanyName, "^ A", RegexOptions.IgnorePatternWhitespace)),
                entryCount: 4);
            AssertContainsSql("WHERE \"c\".\"CompanyName\" ~ ('(?px)' || '^ A')");
        }

        [Fact]
        public void Regex_IsMatchOptionsUnsupported()
        {
            AssertQuery<Customer>(
                cs => cs.Where(c => Regex.IsMatch(c.CompanyName, "^A", RegexOptions.RightToLeft)),
                entryCount: 4);
            Assert.DoesNotContain("WHERE \"c\".\"CompanyName\" ~ ", Fixture.TestSqlLoggerFactory.Sql);
        }
    }
}
