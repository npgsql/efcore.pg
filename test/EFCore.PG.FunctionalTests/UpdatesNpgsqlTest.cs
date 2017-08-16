using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class UpdatesNpgsqlTest : UpdatesRelationalTestBase<UpdatesNpgsqlFixture, NpgsqlTestStore>
    {
        public UpdatesNpgsqlTest(UpdatesNpgsqlFixture fixture)
            : base(fixture)
        {
        }
    }
}
