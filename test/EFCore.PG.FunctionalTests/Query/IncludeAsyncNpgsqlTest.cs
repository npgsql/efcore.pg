using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IncludeAsyncNpgsqlTest : IncludeAsyncTestBase<IncludeNpgsqlFixture>
    {
        public IncludeAsyncNpgsqlTest(IncludeNpgsqlFixture fixture)
            : base(fixture)
        {
        }
    }
}
