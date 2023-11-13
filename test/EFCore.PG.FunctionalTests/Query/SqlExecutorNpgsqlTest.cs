using System.Data.Common;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class SqlExecutorNpgsqlTest : SqlExecutorTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
{
    public SqlExecutorNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture)
        : base(fixture)
    {
    }

    protected override DbParameter CreateDbParameter(string name, object value)
        => new NpgsqlParameter { ParameterName = name, Value = value };

    protected override string TenMostExpensiveProductsSproc
        => @"SELECT * FROM ""Ten Most Expensive Products""()";

    protected override string CustomerOrderHistorySproc
        => @"SELECT * FROM ""CustOrderHist""(@CustomerID)";

    protected override string CustomerOrderHistoryWithGeneratedParameterSproc
        => @"SELECT * FROM ""CustOrderHist""({0})";
}
