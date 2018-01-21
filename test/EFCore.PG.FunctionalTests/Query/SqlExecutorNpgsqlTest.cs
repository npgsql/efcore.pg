using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Npgsql;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
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
    }
}
