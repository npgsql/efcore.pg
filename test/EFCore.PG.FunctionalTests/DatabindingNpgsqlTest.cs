using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class DatabindingNpgsqlTest : DatabindingTestBase<NpgsqlTestStore, F1NpgsqlFixture>
    {
        public DatabindingNpgsqlTest(F1NpgsqlFixture fixture)
            : base(fixture)
        {
        }
    }
}
