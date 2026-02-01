using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class NorthwindDbFunctionsQueryNpgsqlTest : NorthwindDbFunctionsQueryRelationalTestBase<
    NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
{
    // ReSharper disable once UnusedParameter.Local
    public NorthwindDbFunctionsQueryNpgsqlTest(
        NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
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

    [Fact]
    public void String_ILike_negated()
    {
        using var context = CreateContext();
        var count = context.Customers.Count(c => !EF.Functions.ILike(c.ContactName, "%M%"));

        Assert.Equal(57, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "Customers" AS c
WHERE c."ContactName" NOT ILIKE '%M%' OR c."ContactName" IS NULL
""");
    }

    #endregion

    #region Collation

    [MinimumPostgresVersion(12, 0)]
    [PlatformSkipCondition(TestUtilities.Xunit.TestPlatform.Windows, SkipReason = "ICU non-deterministic doesn't seem to work on Windows?")]
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

    protected override string CaseInsensitiveCollation
        => "some-case-insensitive-collation";

    protected override string CaseSensitiveCollation
        => "POSIX";

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
ORDER BY o."OrderDate" <-> TIMESTAMP '1997-06-28T00:00:00' NULLS FIRST
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
        var count = context.Customers.Count(c => EF.Functions.StringToArray(c.ContactName, " ") == new[] { "Maria", "Anders" });

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
        var count = context.Customers.Count(c => EF.Functions.StringToArray(c.ContactName, " ", "Maria") == new[] { null, "Anders" });

        Assert.Equal(1, count);

        AssertSql(
            """
SELECT count(*)::int
FROM "Customers" AS c
WHERE string_to_array(c."ContactName", ' ', 'Maria') = ARRAY[NULL,'Anders']::text[]
""");
    }

    [Fact]
    public void ToDate()
    {
        using var context = CreateContext();
        var count = context.Orders.Count(c => EF.Functions.ToDate(c.OrderDate.ToString(), "YYYY-MM-DD") < new DateOnly(2000, 01, 01));
        Assert.Equal(830, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "Orders" AS o
WHERE to_date(COALESCE(o."OrderDate"::text, ''), 'YYYY-MM-DD') < DATE '2000-01-01'
""");
    }

    [Fact]
    public void ToTimestamp()
    {
        using var context = CreateContext();
        var count = context.Orders.Count(c => EF.Functions.ToTimestamp(c.OrderDate.ToString(), "YYYY-MM-DD") < new DateTime(2000, 01, 01, 0,0,0, DateTimeKind.Utc));
        Assert.Equal(830, count);
        AssertSql(
            """
SELECT count(*)::int
FROM "Orders" AS o
WHERE to_timestamp(COALESCE(o."OrderDate"::text, ''), 'YYYY-MM-DD') < TIMESTAMPTZ '2000-01-01T00:00:00Z'
""");
    }

    #endregion

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);
}
