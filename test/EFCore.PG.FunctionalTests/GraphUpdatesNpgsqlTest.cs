using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.Logging;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class GraphUpdatesNpgsqlTest
        : GraphUpdatesTestBase<GraphUpdatesNpgsqlTest.GraphUpdatesNpgsqlFixture>
    {
        public GraphUpdatesNpgsqlTest(GraphUpdatesNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public class GraphUpdatesNpgsqlFixture : GraphUpdatesFixtureBase
        {
            protected override string StoreName { get; } = "GraphIdentityUpdatesTest";

            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;
        }
    }
}
