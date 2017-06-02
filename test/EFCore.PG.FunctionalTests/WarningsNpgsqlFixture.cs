using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class WarningsNpgsqlFixture : NorthwindQueryNpgsqlFixture
    {
        protected override DbContextOptionsBuilder ConfigureOptions(
            DbContextOptionsBuilder dbContextOptionsBuilder)
            => dbContextOptionsBuilder.ConfigureWarnings(c =>
                c.Throw(RelationalEventId.QueryClientEvaluationWarning));
    }
}
