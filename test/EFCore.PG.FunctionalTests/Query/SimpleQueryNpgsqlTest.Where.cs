using System;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query
{
    public partial class SimpleQueryNpgsqlTest
    {
        public override void Where_string_indexof()
        {
            base.Where_string_indexof();

            AssertContainsSqlFragment("WHERE (STRPOS(c.\"City\", 'Sea') - 1) <> -1");
        }
    }
}
