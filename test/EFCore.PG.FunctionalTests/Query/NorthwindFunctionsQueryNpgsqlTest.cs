using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

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

    public override async Task IsNullOrWhiteSpace_in_predicate(bool async)
    {
        await base.IsNullOrWhiteSpace_in_predicate(async);

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."Region" IS NULL OR btrim(c."Region", E' \t\n\r') = ''
""");
    }

    // PostgreSQL only has log(x, base) over numeric, may be possible to cast back and forth though
    public override Task Where_math_log_new_base(bool async)
        => AssertTranslationFailed(() => base.Where_math_log_new_base(async));

    // PostgreSQL only has log(x, base) over numeric, may be possible to cast back and forth though
    public override Task Where_mathf_log_new_base(bool async)
        => AssertTranslationFailed(() => base.Where_mathf_log_new_base(async));

    // PostgreSQL only has round(v, s) over numeric, may be possible to cast back and forth though
    public override Task Where_mathf_round2(bool async)
        => AssertTranslationFailed(() => base.Where_mathf_round2(async));

    // Convert on DateTime not yet supported
    public override Task Convert_ToString(bool async)
        => AssertTranslationFailed(() => base.Convert_ToString(async));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task String_Join_non_aggregate(bool async)
    {
        var param = "param";
        string nullParam = null;

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => string.Join("|", c.CustomerID, c.CompanyName, param, nullParam, "constant", null)
                    == "ALFKI|Alfreds Futterkiste|param||constant|"));

        AssertSql(
            """
@__param_0='param'

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE concat_ws('|', c."CustomerID", c."CompanyName", COALESCE(@__param_0, ''), COALESCE(NULL, ''), 'constant', '') = 'ALFKI|Alfreds Futterkiste|param||constant|'
""");
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
    public async Task Regex_IsMatch_with_parameter_pattern(bool async)
    {
        var pattern = "^A";

        await AssertQuery(
            async,
            cs => cs.Set<Customer>().Where(c => Regex.IsMatch(c.CompanyName, pattern)));

        AssertSql(
            """
@__pattern_0='^A'

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CompanyName" ~ ('(?p)' || @__pattern_0)
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

    #endregion Regex

    #region Guid

    private static string UuidGenerationFunction { get; } = TestEnvironment.PostgresVersion.AtLeast(13)
        ? "gen_random_uuid"
        : "uuid_generate_v4";

    public override async Task Where_guid_newguid(bool async)
    {
        await base.Where_guid_newguid(async);

        AssertSql(
            $"""
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE {UuidGenerationFunction}() <> '00000000-0000-0000-0000-000000000000'
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task OrderBy_Guid_NewGuid(bool async)
    {
        await AssertQuery(
            async,
            ods => ods.Set<OrderDetail>().OrderBy(od => Guid.NewGuid()).Select(x => x));

        AssertSql(
            $"""
SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY {UuidGenerationFunction}() NULLS FIRST
""");
    }

    #endregion

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

    public override async Task String_Join_over_non_nullable_column(bool async)
    {
        await base.String_Join_over_non_nullable_column(async);

        AssertSql(
            """
SELECT c."City", COALESCE(string_agg(c."CustomerID", '|'), '') AS "Customers"
FROM "Customers" AS c
GROUP BY c."City"
""");
    }

    public override async Task String_Join_over_nullable_column(bool async)
    {
        await base.String_Join_over_nullable_column(async);

        AssertSql(
            """
SELECT c."City", COALESCE(string_agg(COALESCE(c."Region", ''), '|'), '') AS "Regions"
FROM "Customers" AS c
GROUP BY c."City"
""");
    }

    public override async Task String_Join_with_predicate(bool async)
    {
        await base.String_Join_with_predicate(async);

        AssertSql(
            """
SELECT c."City", COALESCE(string_agg(c."CustomerID", '|') FILTER (WHERE length(c."ContactName")::int > 10), '') AS "Customers"
FROM "Customers" AS c
GROUP BY c."City"
""");
    }

    public override async Task String_Join_with_ordering(bool async)
    {
        await base.String_Join_with_ordering(async);

        AssertSql(
            """
SELECT c."City", COALESCE(string_agg(c."CustomerID", '|' ORDER BY c."CustomerID" DESC NULLS LAST), '') AS "Customers"
FROM "Customers" AS c
GROUP BY c."City"
""");
    }

    public override async Task String_Concat(bool async)
    {
        await base.String_Concat(async);

        AssertSql(
            """
SELECT c."City", COALESCE(string_agg(c."CustomerID", ''), '') AS "Customers"
FROM "Customers" AS c
GROUP BY c."City"
""");
    }

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
            @"{ ""Around the Horn"" : ""Thomas Hardy"", ""B's Beverages"" : ""Victoria Ashworth"", ""Consolidated Holdings"" : ""Elizabeth Brown"", ""Eastern Connection"" : ""Ann Devon"", ""North/South"" : ""Simon Crowther"", ""Seven Seas Imports"" : ""Hari Kumar"" }",
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

    #region Unsupported

    // PostgreSQL does not have strpos with starting position
    public override Task Indexof_with_constant_starting_position(bool async)
        => AssertTranslationFailed(() => base.Indexof_with_constant_starting_position(async));

    // PostgreSQL does not have strpos with starting position
    public override Task Indexof_with_parameter_starting_position(bool async)
        => AssertTranslationFailed(() => base.Indexof_with_parameter_starting_position(async));

    // These tests convert (among other things) to and from boolean, which PostgreSQL
    // does not support (https://github.com/dotnet/efcore/issues/19606)
    public override Task Convert_ToBoolean(bool async)
        => Task.CompletedTask;

    public override Task Convert_ToByte(bool async)
        => Task.CompletedTask;

    public override Task Convert_ToDecimal(bool async)
        => Task.CompletedTask;

    public override Task Convert_ToDouble(bool async)
        => Task.CompletedTask;

    public override Task Convert_ToInt16(bool async)
        => Task.CompletedTask;

    public override Task Convert_ToInt64(bool async)
        => Task.CompletedTask;

    #endregion Unsupported

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
