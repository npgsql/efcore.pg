using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public class NorthwindFunctionsQueryNpgsqlTest : NorthwindFunctionsQueryTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
    {
        public NorthwindFunctionsQueryNpgsqlTest(
            NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            ClearLog();
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }
        public override async Task IsNullOrWhiteSpace_in_predicate(bool isAsync)
        {
            await base.IsNullOrWhiteSpace_in_predicate(isAsync);

            AssertSql(
                @"SELECT c.""CustomerID"", c.""Address"", c.""City"", c.""CompanyName"", c.""ContactName"", c.""ContactTitle"", c.""Country"", c.""Fax"", c.""Phone"", c.""PostalCode"", c.""Region""
FROM ""Customers"" AS c
WHERE (c.""Region"" IS NULL) OR (BTRIM(c.""Region"", E' \t\n\r') = '')");
        }

        [ConditionalTheory(Skip = "Fixed for PostgreSQL 12.1, https://www.postgresql.org/message-id/CADT4RqAz7oN4vkPir86Kg1_mQBmBxCp-L_%3D9vRpgSNPJf0KRkw%40mail.gmail.com")]
        public override Task Indexof_with_emptystring(bool isAsync)
            => base.Indexof_with_emptystring(isAsync);

        [Theory(Skip = "PostgreSQL only has log(x, base) over numeric, may be possible to cast back and forth though")]
        public override Task Where_math_log_new_base(bool isAsync)
            => base.Where_math_log_new_base(isAsync);

        [Theory(Skip = "Convert on DateTime not yet supported")]
        public override Task Convert_ToString(bool isAsync)
            => base.Convert_ToString(isAsync);

        #region Substring

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task Substring_without_length_with_Index_of(bool isAsync)
            => AssertQuery(
                isAsync,
                ss => ss.Set<Customer>()
                    .Where(x => x.Address == "Walserweg 21")
                    .Where(x => x.Address.Substring(x.Address.IndexOf("e")) == "erweg 21"),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task Substring_without_length_with_constant(bool isAsync)
            => AssertQuery(
                isAsync,
                //Walserweg 21
                cs => cs.Set<Customer>().Where(x => x.Address.Substring(5) == "rweg 21"),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task Substring_without_length_with_closure(bool isAsync)
        {
            var startIndex = 5;
            return AssertQuery(
                isAsync,
                //Walserweg 21
                ss => ss.Set<Customer>().Where(x => x.Address.Substring(startIndex) == "rweg 21"),
                entryCount: 1);
        }

        #endregion

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

            AssertContainsSqlFragment(@"uuid_generate_v4() <> '00000000-0000-0000-0000-000000000000'");
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

        #region PadLeft, PadRight

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task PadLeft_with_constant(bool isAsync)
            => AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(x => x.Address.PadLeft(20).EndsWith("Walserweg 21")),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task PadLeft_char_with_constant(bool isAsync)
            => AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(x => x.Address.PadLeft(20, 'a').EndsWith("Walserweg 21")),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task PadLeft_with_parameter(bool isAsync)
        {
            var length = 20;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(x => x.Address.PadLeft(length).EndsWith("Walserweg 21")),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task PadLeft_char_with_parameter(bool isAsync)
        {
            var length = 20;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(x => x.Address.PadLeft(length, 'a').EndsWith("Walserweg 21")),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task PadRight_with_constant(bool isAsync)
            => AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(x => x.Address.PadRight(20).StartsWith("Walserweg 21")),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task PadRight_char_with_constant(bool isAsync)
            => AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(x => x.Address.PadRight(20).StartsWith("Walserweg 21")),
                entryCount: 1);

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task PadRight_with_parameter(bool isAsync)
        {
            var length = 20;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(x => x.Address.PadRight(length).StartsWith("Walserweg 21")),
                entryCount: 1);
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public Task PadRight_char_with_parameter(bool isAsync)
        {
            var length = 20;

            return AssertQuery(
                isAsync,
                ss => ss.Set<Customer>().Where(x => x.Address.PadRight(length, 'a').StartsWith("Walserweg 21")),
                entryCount: 1);
        }

        #endregion

        void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();

        void AssertContainsSqlFragment(string expectedFragment)
            => Assert.Contains(Fixture.TestSqlLoggerFactory.SqlStatements, s => s.Contains(expectedFragment));
    }
}
