using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class IncludeAsyncNpgsqlTest : IncludeAsyncTestBase<NorthwindQueryNpgsqlFixture>
    {
        public IncludeAsyncNpgsqlTest(NorthwindQueryNpgsqlFixture fixture)
            : base(fixture)
        {
        }
    }
}
