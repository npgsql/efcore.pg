using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class GraphUpdatesNpgsqlTest : GraphUpdatesNpgsqlTestBase<GraphUpdatesNpgsqlTest.GraphUpdatesNpgsqlFixture>
    {
        public GraphUpdatesNpgsqlTest(GraphUpdatesNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public class GraphUpdatesNpgsqlFixture : GraphUpdatesNpgsqlFixtureBase
        {
            protected override string DatabaseName => "GraphUpdatesTest";
        }
    }
}
