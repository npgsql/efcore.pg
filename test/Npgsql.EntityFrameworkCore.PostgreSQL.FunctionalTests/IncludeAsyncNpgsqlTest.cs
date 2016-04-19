using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.FunctionalTests;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class IncludeAsyncNpgsqlTest : IncludeAsyncTestBase<NorthwindQueryNpgsqlFixture>
    {
        public IncludeAsyncNpgsqlTest(NorthwindQueryNpgsqlFixture fixture)
            : base(fixture)
        {
        }
    }
}
