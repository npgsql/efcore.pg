using System.Data.Common;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query;

public class SqlExecutorNpgsqlTest : SqlExecutorTestBase<NorthwindQueryNpgsqlFixture<NoopModelCustomizer>>
{
    public SqlExecutorNpgsqlTest(NorthwindQueryNpgsqlFixture<NoopModelCustomizer> fixture)
        : base(fixture)
    {
    }

    protected override DbParameter CreateDbParameter(string name, object value)
        => new NpgsqlParameter
        {
            ParameterName = name,
            Value = value
        };

    protected override string TenMostExpensiveProductsSproc => @"SELECT * FROM ""Ten Most Expensive Products""()";

    protected override string CustomerOrderHistorySproc => @"SELECT * FROM ""CustOrderHist""(@CustomerID)";

    protected override string CustomerOrderHistoryWithGeneratedParameterSproc => @"SELECT * FROM ""CustOrderHist""({0})";

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public override Task Executes_stored_procedure_with_generated_parameter(bool async)
    {
        return base.Executes_stored_procedure_with_generated_parameter(async);
    }

    [SkipForCockroachDb("CockroachDB doesn't support function that returns TABLE, https://github.com/cockroachdb/cockroach/issues/100226")]
    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public override Task Executes_stored_procedure_with_parameter(bool async)
    {
        return base.Executes_stored_procedure_with_parameter(async);
    }
}
