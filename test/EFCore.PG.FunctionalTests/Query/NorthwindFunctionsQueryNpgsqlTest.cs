using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindFunctionsQueryNpgsqlTest : NorthwindFunctionsQueryRelationalTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
{
    // ReSharper disable once UnusedParameter.Local
    public NorthwindFunctionsQueryNpgsqlTest(
        NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Substring

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task Substring_without_length_with_Index_of(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(x => x.Address == "Walserweg 21")
                .Where(x => x.Address.Substring(x.Address.IndexOf("e")) == "erweg 21"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task Substring_without_length_with_constant(bool async)
        => AssertQuery(
            async,
            //Walserweg 21
            cs => cs.Set<Customer>().Where(x => x.Address.Substring(5) == "rweg 21"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task Substring_without_length_with_closure(bool async)
    {
        var startIndex = 5;
        return AssertQuery(
            async,
            //Walserweg 21
            ss => ss.Set<Customer>().Where(x => x.Address.Substring(startIndex) == "rweg 21"));
    }

    #endregion

    #region Regex

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Regex_IsMatch_with_constant_pattern(bool async)
    {
        await AssertQuery(
            async,
            cs => cs.Set<Customer>().Where(c => Regex.IsMatch(c.CompanyName, "^A")));

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CompanyName" ~ '(?p)^A'
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Regex_IsMatch_with_constant_pattern_properly_escaped(bool async)
    {
        await AssertQuery(
            async,
            cs => cs.Set<Customer>().Where(c => Regex.IsMatch(c.CompanyName, "^A';foo")),
            assertEmpty: true);

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CompanyName" ~ '(?p)^A'';foo'
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Regex_IsMatch_with_parameter_pattern(bool async)
    {
        var pattern = "^A";

        await AssertQuery(
            async,
            cs => cs.Set<Customer>().Where(c => Regex.IsMatch(c.CompanyName, pattern)));

        AssertSql(
            """
@pattern='^A'

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CompanyName" ~ ('(?p)' || @pattern)
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Regex_IsMatch_negated(bool async)
    {
        await AssertQuery(
            async,
            cs => cs.Set<Customer>().Where(c => !Regex.IsMatch(c.CompanyName, "^A")));

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CompanyName" !~ '(?p)^A'
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Regex_IsMatchOptionsNone(bool async)
    {
        await AssertQuery(
            async,
            cs => cs.Set<Customer>().Where(c => Regex.IsMatch(c.CompanyName, "^A", RegexOptions.None)));

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CompanyName" ~ '(?p)^A'
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Regex_IsMatch_with_IgnoreCase(bool async)
    {
        await AssertQuery(
            async,
            cs => cs.Set<Customer>().Where(c => Regex.IsMatch(c.CompanyName, "^a", RegexOptions.IgnoreCase)));

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CompanyName" ~* '(?p)^a'
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Regex_IsMatch_with_IgnoreCase_negated(bool async)
    {
        await AssertQuery(
            async,
            cs => cs.Set<Customer>().Where(c => !Regex.IsMatch(c.CompanyName, "^a", RegexOptions.IgnoreCase)));

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CompanyName" !~* '(?p)^a'
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Regex_IsMatch_with_Multiline(bool async)
    {
        await AssertQuery(
            async,
            cs => cs.Set<Customer>().Where(c => Regex.IsMatch(c.CompanyName, "^A", RegexOptions.Multiline)));

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CompanyName" ~ '(?n)^A'
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Regex_IsMatch_with_Singleline(bool async)
    {
        await AssertQuery(
            async,
            cs => cs.Set<Customer>().Where(c => Regex.IsMatch(c.CompanyName, "^A", RegexOptions.Singleline)));

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CompanyName" ~ '^A'
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Regex_IsMatch_with_Singleline_and_IgnoreCase(bool async)
    {
        await AssertQuery(
            async,
            cs => cs.Set<Customer>().Where(c => Regex.IsMatch(c.CompanyName, "^a", RegexOptions.Singleline | RegexOptions.IgnoreCase)));

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CompanyName" ~* '^a'
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Regex_IsMatch_with_IgnorePatternWhitespace(bool async)
    {
        await AssertQuery(
            async,
            cs => cs.Set<Customer>().Where(c => Regex.IsMatch(c.CompanyName, "^ A", RegexOptions.IgnorePatternWhitespace)));

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CompanyName" ~ '(?px)^ A'
""");
    }

    [Fact]
    public void Regex_IsMatch_with_unsupported_option()
        => Assert.Throws<InvalidOperationException>(
            () =>
                Fixture.CreateContext().Customers.Where(c => Regex.IsMatch(c.CompanyName, "^A", RegexOptions.RightToLeft)).ToList());

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Regex_Replace_with_constant_pattern_and_replacement(bool async)
    {
        await AssertQuery(
            async,
            source => source.Set<Customer>().Select(x => Regex.Replace(x.CompanyName, "^A", "B")));

        AssertSql(
            """
SELECT regexp_replace(c."CompanyName", '^A', 'B', 'p')
FROM "Customers" AS c
"""
        );
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Regex_Replace_with_parameter_pattern_and_replacement(bool async)
    {
        var pattern = "^A";
        var replacement = "B";

        await AssertQuery(
            async,
            source => source.Set<Customer>().Select(x => Regex.Replace(x.CompanyName, pattern, replacement)));

        AssertSql(
            """
@pattern='^A'
@replacement='B'

SELECT regexp_replace(c."CompanyName", @pattern, @replacement, 'p')
FROM "Customers" AS c
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Regex_Replace_with_OptionsNone(bool async)
    {
        await AssertQuery(
            async,
            source => source.Set<Customer>().Select(x => Regex.Replace(x.CompanyName, "^A", "B", RegexOptions.None)));

        AssertSql(
            """
SELECT regexp_replace(c."CompanyName", '^A', 'B', 'p')
FROM "Customers" AS c
"""
        );
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Regex_Replace_with_IgnoreCase(bool async)
    {
        await AssertQuery(
            async,
            source => source.Set<Customer>().Select(x => Regex.Replace(x.CompanyName, "^a", "B", RegexOptions.IgnoreCase)));

        AssertSql(
            """
SELECT regexp_replace(c."CompanyName", '^a', 'B', 'pi')
FROM "Customers" AS c
"""
        );
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Regex_Replace_with_Multiline(bool async)
    {
        await AssertQuery(
            async,
            source => source.Set<Customer>().Select(x => Regex.Replace(x.CompanyName, "^A", "B", RegexOptions.Multiline)));

        AssertSql(
            """
SELECT regexp_replace(c."CompanyName", '^A', 'B', 'n')
FROM "Customers" AS c
"""
        );
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Regex_Replace_with_Singleline(bool async)
    {
        await AssertQuery(
            async,
            source => source.Set<Customer>().Select(x => Regex.Replace(x.CompanyName, "^A", "B", RegexOptions.Singleline)));

        AssertSql(
            """
SELECT regexp_replace(c."CompanyName", '^A', 'B')
FROM "Customers" AS c
"""
        );
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Regex_Replace_with_Singleline_and_IgnoreCase(bool async)
    {
        await AssertQuery(
            async,
            source => source.Set<Customer>()
                .Select(x => Regex.Replace(x.CompanyName, "^a", "B", RegexOptions.Singleline | RegexOptions.IgnoreCase)));

        AssertSql(
            """
SELECT regexp_replace(c."CompanyName", '^a', 'B', 'i')
FROM "Customers" AS c
"""
        );
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Regex_Replace_with_IgnorePatternWhitespace(bool async)
    {
        await AssertQuery(
            async,
            source => source.Set<Customer>().Select(x => Regex.Replace(x.CompanyName, "^ A", "B", RegexOptions.IgnorePatternWhitespace)));

        AssertSql(
            """
SELECT regexp_replace(c."CompanyName", '^ A', 'B', 'px')
FROM "Customers" AS c
"""
        );
    }

    [Fact]
    public void Regex_Replace_with_unsupported_option()
        => Assert.Throws<InvalidOperationException>(
            () => Fixture.CreateContext().Customers
                .FirstOrDefault(x => Regex.Replace(x.CompanyName, "^A", "foo", RegexOptions.RightToLeft) != null));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    [MinimumPostgresVersion(15, 0)]
    public async Task Regex_Count_with_constant_pattern(bool async)
    {
        await AssertQuery(
            async,
            cs => cs.Set<Customer>().Select(c => Regex.Count(c.CompanyName, "^A")));

        AssertSql(
            """
SELECT regexp_count(c."CompanyName", '^A', 1, 'p')
FROM "Customers" AS c
"""
        );
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    [MinimumPostgresVersion(15, 0)]
    public async Task Regex_Count_with_parameter_pattern(bool async)
    {
        var pattern = "^A";

        await AssertQuery(
            async,
            cs => cs.Set<Customer>().Select(c => Regex.Count(c.CompanyName, pattern)));

        AssertSql(
            """
@pattern='^A'

SELECT regexp_count(c."CompanyName", @pattern, 1, 'p')
FROM "Customers" AS c
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    [MinimumPostgresVersion(15, 0)]
    public async Task Regex_Count_with_OptionsNone(bool async)
    {
        await AssertQuery(
            async,
            cs => cs.Set<Customer>().Select(c => Regex.Count(c.CompanyName, "^A", RegexOptions.None)));

        AssertSql(
            """
SELECT regexp_count(c."CompanyName", '^A', 1, 'p')
FROM "Customers" AS c
"""
        );
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    [MinimumPostgresVersion(15, 0)]
    public async Task Regex_Count_with_IgnoreCase(bool async)
    {
        await AssertQuery(
            async,
            cs => cs.Set<Customer>().Select(c => Regex.Count(c.CompanyName, "^a", RegexOptions.IgnoreCase)));

        AssertSql(
            """
SELECT regexp_count(c."CompanyName", '^a', 1, 'pi')
FROM "Customers" AS c
"""
        );
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    [MinimumPostgresVersion(15, 0)]
    public async Task Regex_Count_with_Multiline(bool async)
    {
        await AssertQuery(
            async,
            cs => cs.Set<Customer>().Select(c => Regex.Count(c.CompanyName, "^A", RegexOptions.Multiline)));

        AssertSql(
            """
SELECT regexp_count(c."CompanyName", '^A', 1, 'n')
FROM "Customers" AS c
"""
        );
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    [MinimumPostgresVersion(15, 0)]
    public async Task Regex_Count_with_Singleline(bool async)
    {
        await AssertQuery(
            async,
            cs => cs.Set<Customer>().Select(c => Regex.Count(c.CompanyName, "^A", RegexOptions.Singleline)));

        AssertSql(
            """
SELECT regexp_count(c."CompanyName", '^A')
FROM "Customers" AS c
"""
        );
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    [MinimumPostgresVersion(15, 0)]
    public async Task Regex_Count_with_Singleline_and_IgnoreCase(bool async)
    {
        await AssertQuery(
            async,
            cs => cs.Set<Customer>().Select(c => Regex.Count(c.CompanyName, "^a", RegexOptions.Singleline | RegexOptions.IgnoreCase)));

        AssertSql(
            """
SELECT regexp_count(c."CompanyName", '^a', 1, 'i')
FROM "Customers" AS c
"""
        );
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    [MinimumPostgresVersion(15, 0)]
    public async Task Regex_Count_with_IgnorePatternWhitespace(bool async)
    {
        await AssertQuery(
            async,
            cs => cs.Set<Customer>().Select(c => Regex.Count(c.CompanyName, "^ A", RegexOptions.IgnorePatternWhitespace)));

        AssertSql(
            """
SELECT regexp_count(c."CompanyName", '^ A', 1, 'px')
FROM "Customers" AS c
"""
        );
    }

    [ConditionalFact]
    [MinimumPostgresVersion(15, 0)]
    public void Regex_Count_with_unsupported_option()
        => Assert.Throws<InvalidOperationException>(
            () => Fixture.CreateContext().Customers
                .FirstOrDefault(x => Regex.Count(x.CompanyName, "^A", RegexOptions.RightToLeft) != 0));

    #endregion Regex

    #region PadLeft, PadRight

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task PadLeft_with_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(x => x.Address.PadLeft(20).EndsWith("Walserweg 21")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task PadLeft_char_with_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(x => x.Address.PadLeft(20, 'a').EndsWith("Walserweg 21")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task PadLeft_with_parameter(bool async)
    {
        var length = 20;

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(x => x.Address.PadLeft(length).EndsWith("Walserweg 21")));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task PadLeft_char_with_parameter(bool async)
    {
        var length = 20;

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(x => x.Address.PadLeft(length, 'a').EndsWith("Walserweg 21")));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task PadRight_with_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(x => x.Address.PadRight(20).StartsWith("Walserweg 21")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task PadRight_char_with_constant(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(x => x.Address.PadRight(20).StartsWith("Walserweg 21")));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task PadRight_with_parameter(bool async)
    {
        var length = 20;

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(x => x.Address.PadRight(length).StartsWith("Walserweg 21")));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task PadRight_char_with_parameter(bool async)
    {
        var length = 20;

        return AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(x => x.Address.PadRight(length, 'a').StartsWith("Walserweg 21")));
    }

    #endregion

    #region Aggregate functions

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_ArrayAgg(bool async)
    {
        await using var ctx = CreateContext();

        var query = ctx.Set<Customer>()
            .GroupBy(c => c.City)
            .Select(g => new { City = g.Key, FaxNumbers = EF.Functions.ArrayAgg(g.Select(c => c.Fax).OrderBy(id => id)) });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        var london = results.Single(x => x.City == "London");
        Assert.Collection(
            london.FaxNumbers,
            Assert.Null,
            f => Assert.Equal("(171) 555-2530", f),
            f => Assert.Equal("(171) 555-3373", f),
            f => Assert.Equal("(171) 555-5646", f),
            f => Assert.Equal("(171) 555-6750", f),
            f => Assert.Equal("(171) 555-9199", f));

        AssertSql(
            """
SELECT c."City", array_agg(c."Fax" ORDER BY c."Fax" NULLS FIRST) AS "FaxNumbers"
FROM "Customers" AS c
GROUP BY c."City"
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_JsonAgg(bool async)
    {
        await using var ctx = CreateContext();

        var query = ctx.Set<Customer>()
            .GroupBy(c => c.City)
            .Select(g => new { City = g.Key, FaxNumbers = EF.Functions.JsonAgg(g.Select(c => c.Fax).OrderBy(id => id)) });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        var london = results.Single(x => x.City == "London");
        Assert.Collection(
            london.FaxNumbers,
            Assert.Null,
            f => Assert.Equal("(171) 555-2530", f),
            f => Assert.Equal("(171) 555-3373", f),
            f => Assert.Equal("(171) 555-5646", f),
            f => Assert.Equal("(171) 555-6750", f),
            f => Assert.Equal("(171) 555-9199", f));

        AssertSql(
            """
SELECT c."City", json_agg(c."Fax" ORDER BY c."Fax" NULLS FIRST) AS "FaxNumbers"
FROM "Customers" AS c
GROUP BY c."City"
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_JsonbAgg(bool async)
    {
        await using var ctx = CreateContext();

        var query = ctx.Set<Customer>()
            .GroupBy(c => c.City)
            .Select(g => new { City = g.Key, FaxNumbers = EF.Functions.JsonbAgg(g.Select(c => c.Fax).OrderBy(id => id)) });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        var london = results.Single(x => x.City == "London");
        Assert.Collection(
            london.FaxNumbers,
            Assert.Null,
            f => Assert.Equal("(171) 555-2530", f),
            f => Assert.Equal("(171) 555-3373", f),
            f => Assert.Equal("(171) 555-5646", f),
            f => Assert.Equal("(171) 555-6750", f),
            f => Assert.Equal("(171) 555-9199", f));

        AssertSql(
            """
SELECT c."City", jsonb_agg(c."Fax" ORDER BY c."Fax" NULLS FIRST) AS "FaxNumbers"
FROM "Customers" AS c
GROUP BY c."City"
""");
    }

    #endregion Aggregate functions

    #region JsonObjectAgg

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_JsonObjectAgg(bool async)
    {
        await using var ctx = CreateContext();

        var query = ctx.Set<Customer>()
            .GroupBy(c => c.City)
            .Select(
                g => new
                {
                    City = g.Key,
                    Companies = EF.Functions.JsonObjectAgg(
                        g
                            .OrderBy(c => c.CompanyName)
                            .Select(c => ValueTuple.Create(c.CompanyName, c.ContactName)))
                });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        var london = results.Single(r => r.City == "London");

        Assert.Equal(
            """{ "Around the Horn" : "Thomas Hardy", "B's Beverages" : "Victoria Ashworth", "Consolidated Holdings" : "Elizabeth Brown", "Eastern Connection" : "Ann Devon", "North/South" : "Simon Crowther", "Seven Seas Imports" : "Hari Kumar" }""",
            london.Companies);

        AssertSql(
            """
SELECT c."City", json_object_agg(c."CompanyName", c."ContactName" ORDER BY c."CompanyName" NULLS FIRST) AS "Companies"
FROM "Customers" AS c
GROUP BY c."City"
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_JsonObjectAgg_as_Dictionary(bool async)
    {
        await using var ctx = CreateContext();

        var query = ctx.Set<Customer>()
            .GroupBy(c => c.City)
            .Select(
                g => new
                {
                    City = g.Key,
                    Companies = EF.Functions.JsonObjectAgg<string, string, Dictionary<string, string>>(
                        g.Select(c => ValueTuple.Create(c.CompanyName, c.ContactName)))
                });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        var london = results.Single(r => r.City == "London");

        Assert.Equal(
            new Dictionary<string, string>
            {
                ["Around the Horn"] = "Thomas Hardy",
                ["B's Beverages"] = "Victoria Ashworth",
                ["Consolidated Holdings"] = "Elizabeth Brown",
                ["Eastern Connection"] = "Ann Devon",
                ["North/South"] = "Simon Crowther",
                ["Seven Seas Imports"] = "Hari Kumar"
            },
            london.Companies);

        AssertSql(
            """
SELECT c."City", json_object_agg(c."CompanyName", c."ContactName") AS "Companies"
FROM "Customers" AS c
GROUP BY c."City"
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_JsonbObjectAgg(bool async)
    {
        await using var ctx = CreateContext();

        // Note that unlike with json, jsonb doesn't guarantee ordering; so we parse the JSON string client-side.
        var query = ctx.Set<Customer>()
            .GroupBy(c => c.City)
            .Select(
                g => new
                {
                    City = g.Key,
                    Companies = EF.Functions.JsonbObjectAgg(g.Select(c => ValueTuple.Create(c.CompanyName, c.ContactName)))
                });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        var london = results.Single(r => r.City == "London");
        var companiesDictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(london.Companies);

        Assert.Equal(
            new Dictionary<string, string>
            {
                ["Around the Horn"] = "Thomas Hardy",
                ["B's Beverages"] = "Victoria Ashworth",
                ["Consolidated Holdings"] = "Elizabeth Brown",
                ["Eastern Connection"] = "Ann Devon",
                ["North/South"] = "Simon Crowther",
                ["Seven Seas Imports"] = "Hari Kumar"
            },
            companiesDictionary);

        AssertSql(
            """
SELECT c."City", jsonb_object_agg(c."CompanyName", c."ContactName") AS "Companies"
FROM "Customers" AS c
GROUP BY c."City"
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task GroupBy_JsonbObjectAgg_as_Dictionary(bool async)
    {
        await using var ctx = CreateContext();

        var query = ctx.Set<Customer>()
            .GroupBy(c => c.City)
            .Select(
                g => new
                {
                    City = g.Key,
                    Companies = EF.Functions.JsonbObjectAgg<string, string, Dictionary<string, string>>(
                        g.Select(c => ValueTuple.Create(c.CompanyName, c.ContactName)))
                });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        var london = results.Single(r => r.City == "London");

        Assert.Equal(
            new Dictionary<string, string>
            {
                ["Around the Horn"] = "Thomas Hardy",
                ["B's Beverages"] = "Victoria Ashworth",
                ["Consolidated Holdings"] = "Elizabeth Brown",
                ["Eastern Connection"] = "Ann Devon",
                ["North/South"] = "Simon Crowther",
                ["Seven Seas Imports"] = "Hari Kumar"
            },
            london.Companies);

        AssertSql(
            """
SELECT c."City", jsonb_object_agg(c."CompanyName", c."ContactName") AS "Companies"
FROM "Customers" AS c
GROUP BY c."City"
""");
    }

    #endregion JsonObjectAgg

    #region Statistics

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task StandardDeviation(bool async)
    {
        await using var ctx = CreateContext();

        var query = ctx.Set<OrderDetail>()
            .GroupBy(od => od.ProductID)
            .Select(
                g => new
                {
                    ProductID = g.Key,
                    SampleStandardDeviation = EF.Functions.StandardDeviationSample(g.Select(od => od.UnitPrice)),
                    PopulationStandardDeviation = EF.Functions.StandardDeviationPopulation(g.Select(od => od.UnitPrice))
                });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        var product9 = results.Single(r => r.ProductID == 9);
        Assert.Equal(8.675943752699023, product9.SampleStandardDeviation.Value, 5);
        Assert.Equal(7.759999999999856, product9.PopulationStandardDeviation.Value, 5);

        AssertSql(
            """
SELECT o."ProductID", stddev_samp(o."UnitPrice") AS "SampleStandardDeviation", stddev_pop(o."UnitPrice") AS "PopulationStandardDeviation"
FROM "Order Details" AS o
GROUP BY o."ProductID"
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Variance(bool async)
    {
        await using var ctx = CreateContext();

        var query = ctx.Set<OrderDetail>()
            .GroupBy(od => od.ProductID)
            .Select(
                g => new
                {
                    ProductID = g.Key,
                    SampleStandardDeviation = EF.Functions.VarianceSample(g.Select(od => od.UnitPrice)),
                    PopulationStandardDeviation = EF.Functions.VariancePopulation(g.Select(od => od.UnitPrice))
                });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        var product9 = results.Single(r => r.ProductID == 9);
        Assert.Equal(75.2719999999972, product9.SampleStandardDeviation.Value, 5);
        Assert.Equal(60.217599999997766, product9.PopulationStandardDeviation.Value, 5);

        AssertSql(
            """
SELECT o."ProductID", var_samp(o."UnitPrice") AS "SampleStandardDeviation", var_pop(o."UnitPrice") AS "PopulationStandardDeviation"
FROM "Order Details" AS o
GROUP BY o."ProductID"
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Other_statistics_functions(bool async)
    {
        await using var ctx = CreateContext();

        var query = ctx.Set<OrderDetail>()
            .GroupBy(od => od.ProductID)
            .Select(
                g => new
                {
                    ProductID = g.Key,
                    Correlation = EF.Functions.Correlation(g.Select(od => ValueTuple.Create((double)od.Quantity, (double)od.Discount))),
                    CovariancePopulation =
                        EF.Functions.CovariancePopulation(g.Select(od => ValueTuple.Create((double)od.Quantity, (double)od.Discount))),
                    CovarianceSample =
                        EF.Functions.CovarianceSample(g.Select(od => ValueTuple.Create((double)od.Quantity, (double)od.Discount))),
                    RegrAverageX = EF.Functions.RegrAverageX(g.Select(od => ValueTuple.Create((double)od.Quantity, (double)od.Discount))),
                    RegrAverageY = EF.Functions.RegrAverageY(g.Select(od => ValueTuple.Create((double)od.Quantity, (double)od.Discount))),
                    RegrCount = EF.Functions.RegrCount(g.Select(od => ValueTuple.Create((double)od.Quantity, (double)od.Discount))),
                    RegrIntercept = EF.Functions.RegrIntercept(g.Select(od => ValueTuple.Create((double)od.Quantity, (double)od.Discount))),
                    RegrR2 = EF.Functions.RegrR2(g.Select(od => ValueTuple.Create((double)od.Quantity, (double)od.Discount))),
                    RegrSlope = EF.Functions.RegrSlope(g.Select(od => ValueTuple.Create((double)od.Quantity, (double)od.Discount))),
                    RegrSXX = EF.Functions.RegrSXX(g.Select(od => ValueTuple.Create((double)od.Quantity, (double)od.Discount))),
                    RegrSXY = EF.Functions.RegrSXY(g.Select(od => ValueTuple.Create((double)od.Quantity, (double)od.Discount))),
                });

        var results = async
            ? await query.ToListAsync()
            : query.ToList();

        var product9 = results.Single(r => r.ProductID == 9);
        Assert.Equal(0.9336470941441423, product9.Correlation.Value, 5);
        Assert.Equal(1.4799999967217445, product9.CovariancePopulation.Value, 5);
        Assert.Equal(1.8499999959021807, product9.CovarianceSample.Value, 5);
        Assert.Equal(0.10000000149011612, product9.RegrAverageX.Value, 5);
        Assert.Equal(19, product9.RegrAverageY.Value, 5);
        Assert.Equal(5, product9.RegrCount.Value);
        Assert.Equal(2.5555555647538144, product9.RegrIntercept.Value, 5);
        Assert.Equal(0.871696896403801, product9.RegrR2.Value, 5);
        Assert.Equal(164.44444190204874, product9.RegrSlope.Value, 5);
        Assert.Equal(0.045000000596046474, product9.RegrSXX.Value, 5);
        Assert.Equal(7.399999983608723, product9.RegrSXY.Value, 5);

        AssertSql(
            """
SELECT o."ProductID", corr(o."Quantity"::double precision, o."Discount"::double precision) AS "Correlation", covar_pop(o."Quantity"::double precision, o."Discount"::double precision) AS "CovariancePopulation", covar_samp(o."Quantity"::double precision, o."Discount"::double precision) AS "CovarianceSample", regr_avgx(o."Quantity"::double precision, o."Discount"::double precision) AS "RegrAverageX", regr_avgy(o."Quantity"::double precision, o."Discount"::double precision) AS "RegrAverageY", regr_count(o."Quantity"::double precision, o."Discount"::double precision) AS "RegrCount", regr_intercept(o."Quantity"::double precision, o."Discount"::double precision) AS "RegrIntercept", regr_r2(o."Quantity"::double precision, o."Discount"::double precision) AS "RegrR2", regr_slope(o."Quantity"::double precision, o."Discount"::double precision) AS "RegrSlope", regr_sxx(o."Quantity"::double precision, o."Discount"::double precision) AS "RegrSXX", regr_sxy(o."Quantity"::double precision, o."Discount"::double precision) AS "RegrSXY"
FROM "Order Details" AS o
GROUP BY o."ProductID"
""");
    }

    #endregion Statistics

    #region NullIf

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task NullIf_with_equality_left_sided(bool async)
    {
        await AssertQuery(
            async,
            cs => cs.Set<Order>().Select(x => x.OrderID == 1 ? (int?)null : x.OrderID));

        AssertSql(
            """
SELECT NULLIF(o."OrderID", 1)
FROM "Orders" AS o
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task NullIf_with_equality_right_sided(bool async)
    {
        await AssertQuery(
            async,
            cs => cs.Set<Order>().Select(x => 1 == x.OrderID ? (int?)null : x.OrderID));

        AssertSql(
            """
SELECT NULLIF(o."OrderID", 1)
FROM "Orders" AS o
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task NullIf_with_inequality_left_sided(bool async)
    {
        await AssertQuery(
            async,
            cs => cs.Set<Order>().Select(x => x.OrderID != 1 ? x.OrderID : (int?)null));

        AssertSql(
            """
SELECT NULLIF(o."OrderID", 1)
FROM "Orders" AS o
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task NullIf_with_inequality_right_sided(bool async)
    {
        await AssertQuery(
            async,
            cs => cs.Set<Order>().Select(x => 1 != x.OrderID ? x.OrderID : (int?)null));

        AssertSql(
            """
SELECT NULLIF(o."OrderID", 1)
FROM "Orders" AS o
""");
    }

    #endregion

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
