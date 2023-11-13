using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class ToSqlQuerySqlServerTest : ToSqlQueryTestBase
{
    protected override ITestStoreFactory TestStoreFactory
        => NpgsqlTestStoreFactory.Instance;

    [ConditionalFact]
    public virtual void Check_all_tests_overridden()
        => TestHelpers.AssertAllMethodsOverridden(GetType());

    // Base test implementation does not properly use identifier delimiters in raw SQL and isn't usable on PostgreSQL
    public override Task Entity_type_with_navigation_mapped_to_SqlQuery(bool async)
        => Task.CompletedTask;

    private void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);
}
