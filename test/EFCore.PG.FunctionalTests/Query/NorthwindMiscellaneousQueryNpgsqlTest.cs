using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit.Sdk;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

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

    // https://github.com/dotnet/efcore/issues/36311
    public override Task Entity_equality_contains_with_list_of_null(bool async)
        => Assert.ThrowsAsync<UnreachableException>(() => base.Entity_equality_contains_with_list_of_null(async));

    public override async Task Query_expression_with_to_string_and_contains(bool async)
    {
        await base.Query_expression_with_to_string_and_contains(async);

        AssertSql(
            """
SELECT o."CustomerID"
FROM "Orders" AS o
WHERE o."OrderDate" IS NOT NULL AND COALESCE(o."EmployeeID"::text, '') LIKE '%7%'
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
@years='2'

SELECT o."OrderDate" + CAST(@years::text || ' years' AS interval) AS "OrderDate"
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
                "Microsoft.EntityFrameworkCore.Query.NorthwindMiscellaneousQueryNpgsqlTest",
                "InstanceMethod"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Client_code_using_instance_method_throws(async))).Message);

        AssertSql();
    }

    public override async Task Client_code_using_instance_in_static_method(bool async)
    {
        Assert.Equal(
            CoreStrings.ClientProjectionCapturingConstantInMethodArgument(
                "Microsoft.EntityFrameworkCore.Query.NorthwindMiscellaneousQueryNpgsqlTest",
                "StaticMethod"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Client_code_using_instance_in_static_method(async))).Message);

        AssertSql();
    }

    public override async Task Client_code_using_instance_in_anonymous_type(bool async)
    {
        Assert.Equal(
            CoreStrings.ClientProjectionCapturingConstantInTree(
                "Microsoft.EntityFrameworkCore.Query.NorthwindMiscellaneousQueryNpgsqlTest"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => base.Client_code_using_instance_in_anonymous_type(async))).Message);

        AssertSql();
    }

    public override async Task Client_code_unknown_method(bool async)
    {
        await AssertTranslationFailedWithDetails(
            () => base.Client_code_unknown_method(async),
            CoreStrings.QueryUnableToTranslateMethod(
                "Microsoft.EntityFrameworkCore.Query.NorthwindMiscellaneousQueryTestBase<Microsoft.EntityFrameworkCore.Query.NorthwindQueryNpgsqlFixture<Microsoft.EntityFrameworkCore.TestUtilities.NoopModelCustomizer>>",
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
SELECT o0."CustomerID", o1."OrderID", o1."CustomerID", o1."EmployeeID", o1."OrderDate"
FROM (
    SELECT DISTINCT o."CustomerID"
    FROM "Orders" AS o
    WHERE o."OrderID" < 10300
) AS o0
LEFT JOIN "Orders" AS o1 ON o0."CustomerID" = o1."CustomerID"
ORDER BY o0."CustomerID" NULLS FIRST
""");
    }

    // TODO: #3406
    public override Task Where_nanosecond_and_microsecond_component(bool async)
        => AssertTranslationFailed(() => base.Where_nanosecond_and_microsecond_component(async));

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
@regions={ 'UK', 'SP' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."Region" = ANY (@regions) OR (c."Region" IS NULL AND array_position(@regions, NULL) IS NOT NULL)
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
@regions={ 'UK', 'SP', NULL } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."Region" = ANY (@regions) OR (c."Region" IS NULL AND array_position(@regions, NULL) IS NOT NULL)
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
        [
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
        ], result.Select(e => e.CustomerID));

        AssertSql(
            """
@collection={ 'A%', 'B%', 'C%' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."Address" LIKE ANY (@collection)
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
@collection={ 'A%', 'B%', 'C%' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."Address" LIKE ALL (@collection)
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Array_All_Like_negated(bool async)
    {
        await using var context = CreateContext();

        var collection = new[] { "A%", "B%", "C%" };
        var query = context.Set<Customer>().Where(c => !collection.All(y => EF.Functions.Like(c.Address, y)));
        var result = async ? await query.ToListAsync() : query.ToList();

        Assert.NotEmpty(result);

        AssertSql(
            """
@collection={ 'A%', 'B%', 'C%' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE NOT (c."Address" LIKE ALL (@collection))
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
        [
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
        ], result.Select(e => e.CustomerID));

        AssertSql(
            """
@collection={ 'a%', 'b%', 'c%' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."Address" ILIKE ANY (@collection)
""");
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Array_Any_ILike_negated(bool async)
    {
        await using var context = CreateContext();

        var collection = new[] { "a%", "b%", "c%" };
        var query = context.Set<Customer>().Where(c => !collection.Any(y => EF.Functions.ILike(c.Address, y)));
        var result = async ? await query.ToListAsync() : query.ToList();

        Assert.Equal(
        [
            "ALFKI",
            "ANTON",
            "AROUT",
            "BLAUS",
            "BLONP"
        ], result.Select(e => e.CustomerID).Order().Take(5));

        AssertSql(
            """
@collection={ 'a%', 'b%', 'c%' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE NOT (c."Address" ILIKE ANY (@collection))
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
@collection={ 'a%', 'b%', 'c%' } (DbType = Object)

SELECT c."CustomerID", c."Address", c."City", c."CompanyName", c."ContactName", c."ContactTitle", c."Country", c."Fax", c."Phone", c."PostalCode", c."Region"
FROM "Customers" AS c
WHERE c."Address" ILIKE ALL (@collection)
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
