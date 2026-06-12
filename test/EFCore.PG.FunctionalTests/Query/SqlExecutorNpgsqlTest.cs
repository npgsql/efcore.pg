using System.Data.Common;

namespace Microsoft.EntityFrameworkCore.Query;

public class SqlExecutorNpgsqlTest : SqlExecutorTestBase<NorthwindQueryNpgsqlFixture<SqlExecutorModelCustomizer>>
{
    public SqlExecutorNpgsqlTest(NorthwindQueryNpgsqlFixture<SqlExecutorModelCustomizer> fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    protected override DbParameter CreateDbParameter(string name, object value)
        => new NpgsqlParameter { ParameterName = name, Value = value };

    protected override string TenMostExpensiveProductsSproc
        => """SELECT * FROM "Ten Most Expensive Products"()""";

    protected override string CustomerOrderHistorySproc
        => """SELECT * FROM "CustOrderHist"(@CustomerID)""";

    protected override string CustomerOrderHistoryWithGeneratedParameterSproc
        => """SELECT * FROM "CustOrderHist"({0})""";
}
