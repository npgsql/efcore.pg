using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit.Sdk;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class NorthwindMiscellaneousQueryNpgsqlTest : NorthwindMiscellaneousQueryRelationalTestBase<
    NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
{
    // ReSharper disable once UnusedParameter.Local
    public NorthwindMiscellaneousQueryNpgsqlTest(
        NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    public override async Task Query_expression_with_to_string_and_contains(bool async)
    {
        await base.Query_expression_with_to_string_and_contains(async);

        AssertSql(
            """
SELECT o."CustomerID"
FROM "Orders" AS o
WHERE o."OrderDate" IS NOT NULL AND o."EmployeeID"::text LIKE '%7%'
""");
    }

    public override async Task Select_expression_date_add_year(bool async)
    {
        await base.Select_expression_date_add_year(async);

        AssertSql(
            """
SELECT o."OrderDate" + INTERVAL '1 years' AS "OrderDate"
FROM "Orders" AS o
WHERE o."OrderDate" IS NOT NULL
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Select_expression_date_add_year_param(bool async)
    {
        var years = 2;

        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate != null)
                .Select(
                    o => new Order { OrderDate = o.OrderDate.Value.AddYears(years) }),
            e => e.OrderDate);

        AssertSql(
            """
@__years_0='2'

SELECT o."OrderDate" + CAST(@__years_0::text || ' years' AS interval) AS "OrderDate"
FROM "Orders" AS o
WHERE o."OrderDate" IS NOT NULL
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task DateTime_subtract_TimeSpan(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate - TimeSpan.FromDays(1) == new DateTime(1997, 10, 8)));

        AssertSql(
            """
SELECT o."OrderID", o."CustomerID", o."EmployeeID", o."OrderDate"
FROM "Orders" AS o
WHERE o."OrderDate" - INTERVAL '1 00:00:00' = TIMESTAMP '1997-10-08T00:00:00'
""");
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public async Task DateTimeFunction_subtract_DateTime(bool async)
    {
        await AssertFirst(
            async,
            ss => ss.Set<Order>().Where(o => o.OrderDate != null)
                .Select(o => new { Elapsed = (DateTime.Today - ((DateTime)o.OrderDate).Date).Days }));

        AssertSql(
            """
SELECT floor(date_part('day', date_trunc('day', now()::timestamp) - date_trunc('day', o."OrderDate")))::int AS "Elapsed"
FROM "Orders" AS o
WHERE o."OrderDate" IS NOT NULL
LIMIT 1
""");
    }

    public override Task Add_minutes_on_constant_value(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Order>().Where(c => c.OrderID < 10500)
                .OrderBy(o => o.OrderID)
                .Select(o => new { Test = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMinutes(o.OrderID % 25) }),
            assertOrder: true,
            elementAsserter: (e, a) => AssertEqual(e.Test, a.Test));

    public override async Task Client_code_using_instance_method_throws(bool async)
    {
        Assert.Equal(
            CoreStrings.ClientProjectionCapturingConstantInMethodInstance(
                "Npgsql.EntityFrameworkCore.PostgreSQL.Query.NorthwindMiscellaneousQueryNpgsqlTest",
                "InstanceMethod"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Client_code_using_instance_method_throws(async))).Message);

        AssertSql();
    }

    public override async Task Client_code_using_instance_in_static_method(bool async)
    {
        Assert.Equal(
            CoreStrings.ClientProjectionCapturingConstantInMethodArgument(
                "Npgsql.EntityFrameworkCore.PostgreSQL.Query.NorthwindMiscellaneousQueryNpgsqlTest",
                "StaticMethod"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Client_code_using_instance_in_static_method(async))).Message);

        AssertSql();
    }

    public override async Task Client_code_using_instance_in_anonymous_type(bool async)
    {
        Assert.Equal(
            CoreStrings.ClientProjectionCapturingConstantInTree(
                "Npgsql.EntityFrameworkCore.PostgreSQL.Query.NorthwindMiscellaneousQueryNpgsqlTest"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Client_code_using_instance_in_anonymous_type(async))).Message);

        AssertSql();
    }

    public override async Task Client_code_unknown_method(bool async)
    {
        await AssertTranslationFailedWithDetails(
            () => base.Client_code_unknown_method(async),
            CoreStrings.QueryUnableToTranslateMethod(
                "Microsoft.EntityFrameworkCore.Query.NorthwindMiscellaneousQueryTestBase<Npgsql.EntityFrameworkCore.PostgreSQL.Query.NorthwindQueryNpgsqlFixture<Microsoft.EntityFrameworkCore.TestUtilities.NoopModelCustomizer>>",
                nameof(UnknownMethod)));

        AssertSql();
    }

    public override async Task Max_on_empty_sequence_throws(bool async)
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => base.Max_on_empty_sequence_throws(async));

        AssertSql(
            """
SELECT (
    SELECT max(o."OrderID")
    FROM "Orders" AS o
    WHERE c."CustomerID" = o."CustomerID") AS "Max"
FROM "Customers" AS c
""");
    }

    public override async Task Entity_equality_through_subquery_composite_key(bool async)
    {
        var message = (await Assert.ThrowsAsync<InvalidOperationException>(
            () => base.Entity_equality_through_subquery_composite_key(async))).Message;

        Assert.Equal(
            CoreStrings.EntityEqualityOnCompositeKeyEntitySubqueryNotSupported("==", nameof(OrderDetail)),
            message);

        AssertSql();
    }

    public override async Task
        Select_DTO_constructor_distinct_with_collection_projection_translated_to_server_with_binding_after_client_eval(bool async)
    {
        // Allow binding of expressions after projection has turned to client eval. Issue #24478.
        await Assert.ThrowsAsync<TrueException>(
            () => base
                .Select_DTO_constructor_distinct_with_collection_projection_translated_to_server_with_binding_after_client_eval(async));

        AssertSql(
            """
SELECT t."CustomerID", o0."OrderID", o0."CustomerID", o0."EmployeeID", o0."OrderDate"
FROM (
    SELECT DISTINCT o."CustomerID"
    FROM "Orders" AS o
    WHERE o."OrderID" < 10300
) AS t
LEFT JOIN "Orders" AS o0 ON t."CustomerID" = o0."CustomerID"
ORDER BY t."CustomerID" NULLS FIRST
""");
    }

    // TODO: Array tests can probably move to the dedicated ArrayQueryTest suite

    #region Array contains

    // Note that this also takes care of array.Any(x => x == y)
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Array_Contains_constant(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => new[] { "ALFKI", "ANATR" }.Contains(c.CustomerID)));

        // Note: for constant lists there's no advantage in using the PostgreSQL-specific x = ANY (a b, c), unlike
        // for parameterized lists.

        AssertSql(
            """
SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."CustomerID" IN ('ALFKI', 'ANATR')
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Array_Contains_parameter(bool async)
    {
        var regions = new[] { "UK", "SP" };

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => regions.Contains(c.Region)));

        // Instead of c."Region" IN ('UK', 'SP') we generate the PostgreSQL-specific x = ANY (a, b, c), which can
        // be parameterized.
        // Ideally parameter sniffing would allow us to produce SQL without the null check since the regions array doesn't contain one
        // (see https://github.com/aspnet/EntityFrameworkCore/issues/17598).
        AssertSql(
            """
@__regions_0={ 'UK', 'SP' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."Region" = ANY (@__regions_0) OR (c."Region" IS NULL AND array_position(@__regions_0, NULL) IS NOT NULL)
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Array_Contains_parameter_with_null(bool async)
    {
        var regions = new[] { "UK", "SP", null };

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => regions.Contains(c.Region)));

        // Instead of c."Region" IN ('UK', 'SP') we generate the PostgreSQL-specific x = ANY (a, b, c), which can
        // be parameterized.
        // Ideally parameter sniffing would allow us to produce SQL with an optimized null check (no need to check the array server-side)
        // (see https://github.com/aspnet/EntityFrameworkCore/issues/17598).
        AssertSql(
            """
@__regions_0={ 'UK', 'SP', NULL } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."Region" = ANY (@__regions_0) OR (c."Region" IS NULL AND array_position(@__regions_0, NULL) IS NOT NULL)
""");
    }

    #endregion Array contains

    #region Any/All Like

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Array_Any_Like(bool async)
    {
        await using var context = CreateContext();

        var collection = new[] { "A%", "B%", "C%" };
        var query = context.Set<Customer>().Where(c => collection.Any(y => EF.Functions.Like(c.Address, y)));
        var result = async ? await query.ToListAsync() : query.ToList();

        Assert.Equal(
            new[]
            {
                "ANATR",
                "BERGS",
                "BOLID",
                "CACTU",
                "COMMI",
                "CONSH",
                "FISSA",
                "FRANK",
                "GODOS",
                "GOURL",
                "HILAA",
                "HUNGC",
                "LILAS",
                "LINOD",
                "PERIC",
                "QUEEN",
                "RANCH",
                "RICAR",
                "SUPRD",
                "TORTU",
                "TRADH",
                "WANDK"
            }, result.Select(e => e.CustomerID));

        AssertSql(
            """
@__collection_0={ 'A%', 'B%', 'C%' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."Address" LIKE ANY (@__collection_0)
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Array_All_Like(bool async)
    {
        await using var context = CreateContext();

        var collection = new[] { "A%", "B%", "C%" };
        var query = context.Set<Customer>().Where(c => collection.All(y => EF.Functions.Like(c.Address, y)));
        var result = async ? await query.ToListAsync() : query.ToList();

        Assert.Empty(result);

        AssertSql(
            """
@__collection_0={ 'A%', 'B%', 'C%' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."Address" LIKE ALL (@__collection_0)
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Array_Any_ILike(bool async)
    {
        await using var context = CreateContext();

        var collection = new[] { "a%", "b%", "c%" };
        var query = context.Set<Customer>().Where(c => collection.Any(y => EF.Functions.ILike(c.Address, y)));
        var result = async ? await query.ToListAsync() : query.ToList();

        Assert.Equal(
            new[]
            {
                "ANATR",
                "BERGS",
                "BOLID",
                "CACTU",
                "COMMI",
                "CONSH",
                "FISSA",
                "FRANK",
                "GODOS",
                "GOURL",
                "HILAA",
                "HUNGC",
                "LILAS",
                "LINOD",
                "PERIC",
                "QUEEN",
                "RANCH",
                "RICAR",
                "SUPRD",
                "TORTU",
                "TRADH",
                "WANDK"
            }, result.Select(e => e.CustomerID));

        AssertSql(
            """
@__collection_0={ 'a%', 'b%', 'c%' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."Address" ILIKE ANY (@__collection_0)
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Array_All_ILike(bool async)
    {
        await using var context = CreateContext();

        var collection = new[] { "a%", "b%", "c%" };
        var query = context.Set<Customer>().Where(c => collection.All(y => EF.Functions.ILike(c.Address, y)));
        var result = async ? await query.ToListAsync() : query.ToList();

        Assert.Empty(result);

        AssertSql(
            """
@__collection_0={ 'a%', 'b%', 'c%' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."Address" ILIKE ALL (@__collection_0)
""");
    }

    #endregion Any/All Like

    [ConditionalFact] // #1560
    public async Task Lateral_join_with_table_is_rewritten_with_subquery()
    {
        await using var ctx = CreateContext();

        _ = await ctx.Customers.Select(c1 => ctx.Customers.Select(c2 => c2.ContactName).ToList()).ToListAsync();

        AssertSql(
            """
SELECT c."CustomerID", c0."ContactName", c0."CustomerID"
FROM "Customers" AS c
LEFT JOIN LATERAL (SELECT * FROM "Customers") AS c0 ON TRUE
ORDER BY c."CustomerID" NULLS FIRST
""");
    }

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
