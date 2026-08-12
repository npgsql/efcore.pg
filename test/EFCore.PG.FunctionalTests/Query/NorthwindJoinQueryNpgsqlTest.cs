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

    // PostgreSQL has no .NET-style char type; char maps to character(1), and casting a digit character to a
    // numeric type parses it ('1' -> 1) rather than yielding its code point like .NET ((uint)'1' -> 49).
    // The base in-memory expectation (empty result) therefore doesn't hold; assert the PostgreSQL semantics
    // while still verifying that the updated closure value is picked up on re-execution.
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
    }

    // Unlike providers where byte[] maps to a scalar binary type and cannot be treated as a collection,
    // Npgsql translates the byte[] parameter as a collection, so the join translates and executes fine
    // (and byte-to-uint comparison semantics match .NET, so the base in-memory expectation holds).
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
    }

    protected override void ClearLog()
        => Fixture.TestSqlLoggerFactory.Clear();
}
