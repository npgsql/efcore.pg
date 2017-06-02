using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class InheritanceNpgsqlTest : InheritanceTestBase<InheritanceNpgsqlFixture>
    {
        public InheritanceNpgsqlTest(InheritanceNpgsqlFixture fixture)
            : base(fixture)
        {
        }
    }
}
