using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Microsoft.EntityFrameworkCore.Query;

public class NorthwindJoinQueryNpgsqlTest : NorthwindJoinQueryRelationalTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
{
    // ReSharper disable once UnusedParameter.Local
    public NorthwindJoinQueryNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        ClearLog();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // #2759
    public override Task Join_local_collection_int_closure_is_cached_correctly(bool async)
        => base.Join_local_collection_int_closure_is_cached_correctly(async);
    // => Assert.ThrowsAsync<InvalidOperationException>(() => base.Join_local_collection_int_closure_is_cached_correctly(async));

    // PostgreSQL casts digit characters to their numeric values, rather than .NET character code points.
    public override async Task Join_local_string_closure_is_cached_correctly(bool async)
    {
        var ids = "12";
        await AssertQueryScalar(
            async,
            ss => from e in ss.Set<Employee>()
                  join id in ids on e.EmployeeID equals id
                  select e.EmployeeID,
            ss => from e in ss.Set<Employee>()
                  where ids.Select(c => (uint)(c - '0')).Contains(e.EmployeeID)
                  select e.EmployeeID);

        ids = "3";
        await AssertQueryScalar(
            async,
            ss => from e in ss.Set<Employee>()
                  join id in ids on e.EmployeeID equals id
                  select e.EmployeeID,
            ss => from e in ss.Set<Employee>()
                  where ids.Select(c => (uint)(c - '0')).Contains(e.EmployeeID)
                  select e.EmployeeID);

        AssertSql(
            """
@p={ '1'
'2' } (DbType = Object)

SELECT e."EmployeeID"
FROM "Employees" AS e
INNER JOIN unnest(@p) AS p(value) ON e."EmployeeID" = p.value::int
""",
            //
            """
@p={ '3' } (DbType = Object)

SELECT e."EmployeeID"
FROM "Employees" AS e
INNER JOIN unnest(@p) AS p(value) ON e."EmployeeID" = p.value::int
""");
    }

    // Npgsql supports joining a byte[] parameter as a collection.
    public override async Task Join_local_bytes_closure_is_cached_correctly(bool async)
    {
        var ids = new byte[] { 1, 2 };
        await AssertQueryScalar(
            async,
            ss => from e in ss.Set<Employee>()
                  join id in ids on e.EmployeeID equals id
                  select e.EmployeeID);

        ids = [3];
        await AssertQueryScalar(
            async,
            ss => from e in ss.Set<Employee>()
                  join id in ids on e.EmployeeID equals id
                  select e.EmployeeID);

        AssertSql(
            """
@p='0x0102' (DbType = Object)

SELECT e."EmployeeID"
FROM "Employees" AS e
INNER JOIN unnest(@p) AS p(value) ON e."EmployeeID" = p.value::int
""",
            //
            """
@p='0x03' (DbType = Object)

SELECT e."EmployeeID"
FROM "Employees" AS e
INNER JOIN unnest(@p) AS p(value) ON e."EmployeeID" = p.value::int
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
