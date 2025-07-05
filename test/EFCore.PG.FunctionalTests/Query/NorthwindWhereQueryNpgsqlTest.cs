using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindWhereQueryNpgsqlTest : NorthwindWhereQueryRelationalTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
{
    // ReSharper disable once UnusedParameter.Local
    public NorthwindWhereQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // https://github.com/dotnet/efcore/issues/36311
    public override Task Where_navigation_contains(bool async)
        => Assert.ThrowsAsync<UnreachableException>(() => base.Where_navigation_contains(async));

    public override async Task Where_compare_constructed_equal(bool async)
    {
        // Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_constructed_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_constructed_multi_value_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_constructed_multi_value_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_constructed_multi_value_not_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_constructed_multi_value_not_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_constructed_equal(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => new Tuple<string>(c.City).Equals(new Tuple<string>("London"))));

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE (c."City") = ('London')
""");
    }

    public override async Task Where_compare_tuple_constructed_multi_value_equal(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => new Tuple<string, string>(c.City, c.Country).Equals(new Tuple<string, string>("Sao Paulo", "Brazil"))));

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE (c."City", c."Country") = ('Sao Paulo', 'Brazil')
""");
    }

    public override async Task Where_compare_tuple_constructed_multi_value_not_equal(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(
                c => !new Tuple<string, string>(c.City, c.Country).Equals(new Tuple<string, string>("Sao Paulo", "Brazil"))));

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."City" <> 'Sao Paulo' OR c."City" IS NULL OR c."Country" <> 'Brazil' OR c."Country" IS NULL
""");
    }

    public override async Task Where_compare_tuple_create_constructed_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_tuple_create_constructed_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_create_constructed_multi_value_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_tuple_create_constructed_multi_value_equal(async));

        AssertSql();
    }

    public override async Task Where_compare_tuple_create_constructed_multi_value_not_equal(bool async)
    {
        //  Anonymous type to constant comparison. Issue #14672.
        await AssertTranslationFailed(() => base.Where_compare_tuple_create_constructed_multi_value_not_equal(async));

        AssertSql();
    }

    #region Row values

    [ConditionalFact]
    public async Task Row_value_GreaterThan()
    {
        await using var ctx = CreateContext();

        _ = await ctx.Customers
            .Where(
                c => EF.Functions.GreaterThan(
                    ValueTuple.Create(c.City, c.CustomerID),
                    ValueTuple.Create("Buenos Aires", "OCEAN")))
            .CountAsync();

        AssertSql(
            """
SELECT count(*)::int
FROM "Customers" AS c
WHERE (c."City", c."CustomerID") > ('Buenos Aires', 'OCEAN')
""");
    }

    [ConditionalFact]
    public async Task Row_value_GreaterThan_with_differing_types()
    {
        await using var ctx = CreateContext();

        _ = await ctx.Orders
            .Where(
                o => EF.Functions.GreaterThan(
                    ValueTuple.Create(o.CustomerID, o.OrderID),
                    ValueTuple.Create("ALFKI", 10702)))
            .CountAsync();

        AssertSql(
            """
SELECT count(*)::int
FROM "Orders" AS o
WHERE (o."CustomerID", o."OrderID") > ('ALFKI', 10702)
""");
    }

    [ConditionalFact]
    public async Task Row_value_GreaterThan_with_parameter()
    {
        await using var ctx = CreateContext();

        var city1 = "Buenos Aires";

        _ = await ctx.Customers
            .Where(
                c => EF.Functions.GreaterThan(
                    ValueTuple.Create(c.City, c.CustomerID),
                    ValueTuple.Create(city1, "OCEAN")))
            .CountAsync();

        AssertSql(
            """
@city1='Buenos Aires'

SELECT count(*)::int
FROM "Customers" AS c
WHERE (c."City", c."CustomerID") > (@city1, 'OCEAN')
""");
    }

    [ConditionalFact]
    public async Task Row_value_GreaterThan_with_parameter_with_ValueTuple_constructor()
    {
        await using var ctx = CreateContext();

        var city1 = "Buenos Aires";

        _ = await ctx.Customers
            .Where(
                c => EF.Functions.GreaterThan(
                    new ValueTuple<string, string>(c.City, c.CustomerID),
                    new ValueTuple<string, string>(city1, "OCEAN")))
            .CountAsync();

        AssertSql(
            """
@city1='Buenos Aires'

SELECT count(*)::int
FROM "Customers" AS c
WHERE (c."City", c."CustomerID") > (@city1, 'OCEAN')
""");
    }

    [ConditionalFact]
    public async Task Row_value_LessThan()
    {
        await using var ctx = CreateContext();

        _ = await ctx.Customers
            .Where(
                c => EF.Functions.LessThan(
                    ValueTuple.Create(c.City, c.CustomerID),
                    ValueTuple.Create("Buenos Aires", "OCEAN")))
            .CountAsync();

        AssertSql(
            """
SELECT count(*)::int
FROM "Customers" AS c
WHERE (c."City", c."CustomerID") < ('Buenos Aires', 'OCEAN')
""");
    }

    [ConditionalFact]
    public async Task Row_value_GreaterThanOrEqual()
    {
        await using var ctx = CreateContext();

        _ = await ctx.Customers
            .Where(
                c => EF.Functions.GreaterThanOrEqual(
                    ValueTuple.Create(c.City, c.CustomerID),
                    ValueTuple.Create("Buenos Aires", "OCEAN")))
            .CountAsync();

        AssertSql(
            """
SELECT count(*)::int
FROM "Customers" AS c
WHERE (c."City", c."CustomerID") >= ('Buenos Aires', 'OCEAN')
""");
    }

    [ConditionalFact]
    public async Task Row_value_LessThanOrEqual()
    {
        await using var ctx = CreateContext();

        _ = await ctx.Customers
            .Where(
                c => EF.Functions.LessThanOrEqual(
                    ValueTuple.Create(c.City, c.CustomerID),
                    ValueTuple.Create("Buenos Aires", "OCEAN")))
            .CountAsync();

        AssertSql(
            """
SELECT count(*)::int
FROM "Customers" AS c
WHERE (c."City", c."CustomerID") <= ('Buenos Aires', 'OCEAN')
""");
    }

    [ConditionalFact]
    public async Task Row_value_with_ValueTuple_constructor()
    {
        await using var ctx = CreateContext();

        _ = await ctx.Customers
            .Where(
                c => EF.Functions.GreaterThan(
                    new ValueTuple<string, string>(c.City, c.CustomerID),
                    new ValueTuple<string, string>("Buenos Aires", "OCEAN")))
            .CountAsync();

        AssertSql(
            """
SELECT count(*)::int
FROM "Customers" AS c
WHERE (c."City", c."CustomerID") > ('Buenos Aires', 'OCEAN')
""");
    }

    [ConditionalFact]
    public async Task Row_value_parameter_count_mismatch()
    {
        await using var ctx = CreateContext();

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => ctx.Customers
                .Where(
                    c => EF.Functions.LessThanOrEqual(
                        ValueTuple.Create(c.City, c.CustomerID),
                        ValueTuple.Create("Buenos Aires", "OCEAN", "foo")))
                .CountAsync());

        Assert.Equal(NpgsqlStrings.RowValueComparisonRequiresTuplesOfSameLength, exception.Message);
    }

    [ConditionalFact]
    public async Task Row_value_equals()
    {
        await using var ctx = CreateContext();

        _ = await ctx.Customers
            .Where(
                c =>
                    ValueTuple.Create(c.City, c.CustomerID).Equals(
                        ValueTuple.Create("Buenos Aires", "OCEAN")))
            .CountAsync();

        AssertSql(
            """
SELECT count(*)::int
FROM "Customers" AS c
WHERE (c."City", c."CustomerID") = ('Buenos Aires', 'OCEAN')
""");
    }

    [ConditionalFact]
    public async Task Row_value_not_equals()
    {
        await using var ctx = CreateContext();

        // Everything is non-nullable, so we use the nicer row value comparison syntax
        _ = await ctx.Customers
            .Where(c => !ValueTuple.Create(c.CustomerID, c.CustomerID).Equals(ValueTuple.Create("OCEAN", "OCEAN")))
            .CountAsync();

        AssertSql(
            """
SELECT count(*)::int
FROM "Customers" AS c
WHERE (c."CustomerID", c."CustomerID") <> ('OCEAN', 'OCEAN')
""");
    }

    [ConditionalFact]
    public async Task Row_value_not_equals_with_nullable()
    {
        await using var ctx = CreateContext();

        _ = await ctx.Customers
            .Where(c => !ValueTuple.Create(c.City, c.CustomerID).Equals(ValueTuple.Create("Buenos Aires", "OCEAN")))
            .CountAsync();

        // City is nullable, so we must extract that comparison out of the row value
        AssertSql(
            """
SELECT count(*)::int
FROM "Customers" AS c
WHERE c."CustomerID" <> 'OCEAN' OR c."City" <> 'Buenos Aires' OR c."City" IS NULL
""");
    }

    [ConditionalFact]
    public async Task Row_value_project()
    {
        await using var ctx = CreateContext();

        var (customerId, orderDate) = await ctx.Orders
            .Where(o => o.OrderID == 10248)
            .Select(o => ValueTuple.Create(o.CustomerID, o.OrderDate))
            .SingleAsync();

        Assert.Equal("VINET", customerId);
        Assert.Equal(new DateTime(1996, 7, 4), orderDate);

        AssertSql(
            """
SELECT (o."CustomerID", o."OrderDate")
FROM "Orders" AS o
WHERE o."OrderID" = 10248
LIMIT 2
""");
    }

    #endregion Row values

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
