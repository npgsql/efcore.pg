using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public partial class SimpleQueryNpgsqlTest
    {
        public override async Task IsNullOrWhiteSpace_in_predicate(bool isAsync)
        {
            await base.IsNullOrWhiteSpace_in_predicate(isAsync);

            AssertSql(
                @"SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
WHERE (c.""Region"" IS NULL) OR ((BTRIM(c.""Region"", E' \t\n\r') = '') AND (BTRIM(c.""Region"", E' \t\n\r') IS NOT NULL))");
        }

        public override async Task Query_expression_with_to_string_and_contains(bool isAsync)
        {
            await base.Query_expression_with_to_string_and_contains(isAsync);
            AssertContainsSqlFragment(@"STRPOS(CAST(o.""EmployeeID"" AS text), '10') > 0");
        }

        [ConditionalTheory(Skip = "Fixed for PostgreSQL 12.1, https://www.postgresql.org/message-id/CADT4RqAz7oN4vkPir86Kg1_mQBmBxCp-L_%3D9vRpgSNPJf0KRkw%40mail.gmail.com")]
        public override Task Indexof_with_emptystring(bool isAsync)
            => base.Indexof_with_emptystring(isAsync);

        #region Regex

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Regex_IsMatch(bool isAsync)
        {
            await AssertQuery(
                isAsync,
                cs => cs.Set<Customer>().Where(c => Regex.IsMatch(c.CompanyName, "^A")),
                entryCount: 4);

            AssertContainsSqlFragment(@"WHERE c.""CompanyName"" ~ ('(?p)' || '^A')");
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Regex_IsMatchOptionsNone(bool isAsync)
        {
            await AssertQuery(
                isAsync,
                cs => cs.Set<Customer>().Where(c => Regex.IsMatch(c.CompanyName, "^A", RegexOptions.None)),
                entryCount: 4);

            AssertContainsSqlFragment(@"WHERE c.""CompanyName"" ~ ('(?p)' || '^A')");
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Regex_IsMatchOptionsIgnoreCase(bool isAsync)
        {
            await AssertQuery(
                isAsync,
                cs => cs.Set<Customer>().Where(c => Regex.IsMatch(c.CompanyName, "^a", RegexOptions.IgnoreCase)),
                entryCount: 4);

            AssertContainsSqlFragment(@"WHERE c.""CompanyName"" ~ ('(?ip)' || '^a')");
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Regex_IsMatchOptionsMultiline(bool isAsync)
        {
            await AssertQuery(
                isAsync,
                cs => cs.Set<Customer>().Where(c => Regex.IsMatch(c.CompanyName, "^A", RegexOptions.Multiline)),
                entryCount: 4);

            AssertContainsSqlFragment(@"WHERE c.""CompanyName"" ~ ('(?n)' || '^A')");
        }

        // ReSharper disable once IdentifierTypo
        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Regex_IsMatchOptionsSingleline(bool isAsync)
        {
            await AssertQuery(
                isAsync,
                cs => cs.Set<Customer>().Where(c => Regex.IsMatch(c.CompanyName, "^A", RegexOptions.Singleline)),
                entryCount: 4);

            AssertContainsSqlFragment(@"WHERE c.""CompanyName"" ~ '^A'");
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public async Task Regex_IsMatchOptionsIgnorePatternWhitespace(bool isAsync)
        {
            await AssertQuery(
                isAsync,
                cs => cs.Set<Customer>().Where(c => Regex.IsMatch(c.CompanyName, "^ A", RegexOptions.IgnorePatternWhitespace)),
                entryCount: 4);

            AssertContainsSqlFragment(@"WHERE c.""CompanyName"" ~ ('(?px)' || '^ A')");
        }

        [Fact]
        public void Regex_IsMatchOptionsUnsupported()
            => Assert.Throws<InvalidOperationException>(() =>
                Fixture.CreateContext().Customers.Where(c => Regex.IsMatch(c.CompanyName, "^A", RegexOptions.RightToLeft)).ToList());

        #endregion Regex

        #region Guid

        public override async Task Where_guid_newguid(bool isAsync)
        {
            await base.Where_guid_newguid(isAsync);

            AssertContainsSqlFragment(@"WHERE uuid_generate_v4() <> '00000000-0000-0000-0000-000000000000'");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task OrderBy_Guid_NewGuid(bool isAsync)
        {
            await AssertQuery(
                isAsync,
                ods => ods.Set<OrderDetail>().OrderBy(od => Guid.NewGuid()).Select(x => x),
                entryCount: 2155);

            AssertContainsSqlFragment(@"ORDER BY uuid_generate_v4()");
        }

        #endregion

        void AssertContainsSqlFragment(string expectedFragment)
            => Assert.Contains(Fixture.TestSqlLoggerFactory.SqlStatements, s => s.Contains(expectedFragment));
    }
}
