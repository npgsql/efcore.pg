using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class NorthwindDbFunctionsQueryNpgsqlTest : NorthwindDbFunctionsQueryRelationalTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
{
    // ReSharper disable once UnusedParameter.Local
    public NorthwindDbFunctionsQueryNpgsqlTest(
        NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    #region Like / ILike

    public override async Task Like_literal(bool async)
    {
        // PostgreSQL like is case-sensitive, while the EF Core "default" (i.e. SqlServer) is insensitive.
        // So we override and assert only 19 matches unlike the default's 34.
        await AssertCount(
            async,
            ss => ss.Set<Customer>(),
            ss => ss.Set<Customer>(),
            c => EF.Functions.Like(c.ContactName, "%M%"),
            c => c.ContactName.Contains("M"));

        AssertSql(
"""
SELECT count(*)::int
FROM "Customers" AS c
WHERE c."ContactName" LIKE '%M%'
""");
    }

    public override async Task Like_identity(bool async)
    {
        await base.Like_identity(async);

        AssertSql(
"""
SELECT count(*)::int
FROM "Customers" AS c
WHERE c."ContactName" LIKE c."ContactName" ESCAPE ''
""");
    }

    public override async Task Like_literal_with_escape(bool async)
    {
        await base.Like_literal_with_escape(async);

        AssertSql(
"""
SELECT count(*)::int
FROM "Customers" AS c
WHERE c."ContactName" LIKE '!%' ESCAPE '!'
""");
    }

    [Fact]
    public void String_Like_Literal_With_Backslash()
    {
        using var context = CreateContext();
        var count = context.Customers.Count(c => EF.Functions.Like(c.ContactName, "\\"));

        Assert.Equal(0, count);
        AssertSql(
"""
SELECT count(*)::int
FROM "Customers" AS c
WHERE c."ContactName" LIKE '\' ESCAPE ''
""");
    }

    [Fact]
    public void String_ILike_Literal()
    {
        using var context = CreateContext();
        var count = context.Customers.Count(c => EF.Functions.ILike(c.ContactName, "%M%"));

        Assert.Equal(34, count);
        AssertSql(
"""
SELECT count(*)::int
FROM "Customers" AS c
WHERE c."ContactName" ILIKE '%M%'
""");
    }

    [Fact]
    public void String_ILike_Literal_With_Escape()
    {
        using var context = CreateContext();
        var count = context.Customers.Count(c => EF.Functions.ILike(c.ContactName, "!%", "!"));

        Assert.Equal(0, count);
        AssertSql(
"""
SELECT count(*)::int
FROM "Customers" AS c
WHERE c."ContactName" ILIKE '!%' ESCAPE '!'
""");
    }

    #endregion

    #region Collation

    [MinimumPostgresVersion(12, 0)]
    [PlatformSkipCondition(TestPlatform.Windows, SkipReason = "ICU non-deterministic doesn't seem to work on Windows?")]
    public override async Task Collate_case_insensitive(bool async)
    {
        await base.Collate_case_insensitive(async);

        AssertSql(
"""
SELECT count(*)::int
FROM "Customers" AS c
WHERE c."ContactName" COLLATE "some-case-insensitive-collation" = 'maria anders'
""");
    }

    public override async Task Collate_case_sensitive(bool async)
    {
        await base.Collate_case_sensitive(async);

        AssertSql(
"""
SELECT count(*)::int
FROM "Customers" AS c
WHERE c."ContactName" COLLATE "POSIX" = 'maria anders'
""");
    }

    protected override string CaseInsensitiveCollation => "some-case-insensitive-collation";
    protected override string CaseSensitiveCollation => "POSIX";

    #endregion Collation

    #region Others

    [Fact]
    public void Distance_with_timestamp()
    {
        using var context = CreateContext();
        var closestOrder = context.Orders.OrderBy(o => EF.Functions.Distance(o.OrderDate.Value, new DateTime(1997, 06, 28))).First();

        Assert.Equal(10582, closestOrder.OrderID);

        AssertSql(
"""
SELECT o."OrderID", o."CustomerID", o."EmployeeID", o."OrderDate"
FROM "Orders" AS o
ORDER BY o."OrderDate" <-> TIMESTAMP '1997-06-28 00:00:00' NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void String_reverse()
    {
        using var context = CreateContext();
        var count = context.Customers.Count(c => EF.Functions.Reverse(c.ContactName) == "srednA airaM");

        Assert.Equal(1, count);

        AssertSql(
"""
SELECT count(*)::int
FROM "Customers" AS c
WHERE reverse(c."ContactName") = 'srednA airaM'
""");
    }

    // ReSharper disable once InconsistentNaming
    [Fact]
    public void StringToArray()
    {
        using var context = CreateContext();
        var count = context.Customers.Count(c => EF.Functions.StringToArray(c.ContactName, " ") == new[] { "Maria", "Anders"});

        Assert.Equal(1, count);

        AssertSql(
"""
SELECT count(*)::int
FROM "Customers" AS c
WHERE string_to_array(c."ContactName", ' ') = ARRAY['Maria','Anders']::text[]
""");
    }

    [Fact]
    public void StringToArray_with_null_string()
    {
        using var context = CreateContext();
        var count = context.Customers.Count(c => EF.Functions.StringToArray(c.ContactName, " ", "Maria") == new[] { null, "Anders"});

        Assert.Equal(1, count);

        AssertSql(
"""
SELECT count(*)::int
FROM "Customers" AS c
WHERE string_to_array(c."ContactName", ' ', 'Maria') = ARRAY[NULL,'Anders']::text[]
""");
    }

    [Fact]
    public void NullIf_returns_not_null()
    {
        using var context = CreateContext();
        var nameNotNull = context.Customers
            .Where(c => c.CustomerID == "ALFKI")
            .Select(c => EF.Functions.NullIf(c.ContactName, "Something"))
            .First();

        Assert.NotNull(nameNotNull);

        AssertSql("""
SELECT nullif(c."ContactName", 'Something')
FROM "Customers" AS c
WHERE c."CustomerID" = 'ALFKI'
LIMIT 1
""");
    }

    [Fact]
    public void NullIf_returns_null()
    {
        using var context = CreateContext();
        var nameNull = context.Customers
            .Where(c => c.CustomerID == "ALFKI")
            .Select(c => EF.Functions.NullIf(c.ContactName, "Maria Anders"))
            .First();

        Assert.Null(nameNull);

        AssertSql("""
SELECT nullif(c."ContactName", 'Maria Anders')
FROM "Customers" AS c
WHERE c."CustomerID" = 'ALFKI'
LIMIT 1
""");
    }

    #endregion

    #region Greatest

    [Fact]
    public void Greatest_with_1_property()
    {
        using var context = CreateContext();
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Greatest(o.ProductID)).First();

        //Assert.Equal(10582, closestOrder.OrderID);

        AssertSql(
"""
SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY greatest(o."ProductID") NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Greatest_with_2_properties()
    {
        using var context = CreateContext();
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Greatest(o.ProductID, o.OrderID)).First();

        AssertSql(
"""
SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY greatest(o."ProductID", o."OrderID") NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Greatest_with_3_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Greatest(o.ProductID, value, 2)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY greatest(o."ProductID", @__value_1, 2) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Greatest_with_4_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Greatest(o.ProductID, value, 2, 3)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY greatest(o."ProductID", @__value_1, 2, 3) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Greatest_with_5_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Greatest(o.ProductID, value, 2, 3, 4)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY greatest(o."ProductID", @__value_1, 2, 3, 4) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Greatest_with_6_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Greatest(o.ProductID, value, 2, 3, 4, 5)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY greatest(o."ProductID", @__value_1, 2, 3, 4, 5) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Greatest_with_7_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Greatest(o.ProductID, value, 2, 3, 4, 5, 6)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY greatest(o."ProductID", @__value_1, 2, 3, 4, 5, 6) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Greatest_with_8_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Greatest(o.ProductID, value, 2, 3, 4, 5, 6, 7)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY greatest(o."ProductID", @__value_1, 2, 3, 4, 5, 6, 7) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Greatest_with_9_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Greatest(o.ProductID, value, 2, 3, 4, 5, 6, 7, 8)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY greatest(o."ProductID", @__value_1, 2, 3, 4, 5, 6, 7, 8) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Greatest_with_10_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Greatest(o.ProductID, value, 2, 3, 4, 5, 6, 7, 8, 9)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY greatest(o."ProductID", @__value_1, 2, 3, 4, 5, 6, 7, 8, 9) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Greatest_with_11_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Greatest(o.ProductID, value, 2, 3, 4, 5, 6, 7, 8, 9, 10)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY greatest(o."ProductID", @__value_1, 2, 3, 4, 5, 6, 7, 8, 9, 10) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Greatest_with_12_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Greatest(o.ProductID, value, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY greatest(o."ProductID", @__value_1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Greatest_with_13_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Greatest(o.ProductID, value, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY greatest(o."ProductID", @__value_1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Greatest_with_14_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Greatest(o.ProductID, value, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY greatest(o."ProductID", @__value_1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Greatest_with_15_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Greatest(o.ProductID, value, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY greatest(o."ProductID", @__value_1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Greatest_with_16_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Greatest(o.ProductID, value, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY greatest(o."ProductID", @__value_1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15) NULLS FIRST
LIMIT 1
""");
    }
    #endregion

    #region Least

    [Fact]
    public void Least_with_1_property()
    {
        using var context = CreateContext();
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Least(o.ProductID)).First();

        //Assert.Equal(10582, closestOrder.OrderID);

        AssertSql(
"""
SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY least(o."ProductID") NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Least_with_2_properties()
    {
        using var context = CreateContext();
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Least(o.ProductID, o.OrderID)).First();

        AssertSql(
"""
SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY least(o."ProductID", o."OrderID") NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Least_with_3_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Least(o.ProductID, value, 2)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY least(o."ProductID", @__value_1, 2) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Least_with_4_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Least(o.ProductID, value, 2, 3)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY least(o."ProductID", @__value_1, 2, 3) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Least_with_5_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Least(o.ProductID, value, 2, 3, 4)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY least(o."ProductID", @__value_1, 2, 3, 4) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Least_with_6_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Least(o.ProductID, value, 2, 3, 4, 5)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY least(o."ProductID", @__value_1, 2, 3, 4, 5) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Least_with_7_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Least(o.ProductID, value, 2, 3, 4, 5, 6)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY least(o."ProductID", @__value_1, 2, 3, 4, 5, 6) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Least_with_8_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Least(o.ProductID, value, 2, 3, 4, 5, 6, 7)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY least(o."ProductID", @__value_1, 2, 3, 4, 5, 6, 7) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Least_with_9_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Least(o.ProductID, value, 2, 3, 4, 5, 6, 7, 8)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY least(o."ProductID", @__value_1, 2, 3, 4, 5, 6, 7, 8) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Least_with_10_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Least(o.ProductID, value, 2, 3, 4, 5, 6, 7, 8, 9)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY least(o."ProductID", @__value_1, 2, 3, 4, 5, 6, 7, 8, 9) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Least_with_11_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Least(o.ProductID, value, 2, 3, 4, 5, 6, 7, 8, 9, 10)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY least(o."ProductID", @__value_1, 2, 3, 4, 5, 6, 7, 8, 9, 10) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Least_with_12_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Least(o.ProductID, value, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY least(o."ProductID", @__value_1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Least_with_13_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Least(o.ProductID, value, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY least(o."ProductID", @__value_1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Least_with_14_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Least(o.ProductID, value, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY least(o."ProductID", @__value_1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Least_with_15_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Least(o.ProductID, value, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY least(o."ProductID", @__value_1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14) NULLS FIRST
LIMIT 1
""");
    }

    [Fact]
    public void Least_with_16_mix()
    {
        using var context = CreateContext();
        var value = 1;
        var orderDetail = context.OrderDetails.OrderBy(o => EF.Functions.Least(o.ProductID, value, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15)).First();

        AssertSql(
"""
@__value_1='1'

SELECT o."OrderID", o."ProductID", o."Discount", o."Quantity", o."UnitPrice"
FROM "Order Details" AS o
ORDER BY least(o."ProductID", @__value_1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15) NULLS FIRST
LIMIT 1
""");
    }
    #endregion

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
