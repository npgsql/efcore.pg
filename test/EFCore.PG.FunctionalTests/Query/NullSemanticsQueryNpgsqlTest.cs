using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NullSemanticsQueryNpgsqlTest : NullSemanticsQueryTestBase<NpgsqlTestStore, NullSemanticsQueryNpgsqlFixture>
    {
        public NullSemanticsQueryNpgsqlTest(NullSemanticsQueryNpgsqlFixture fixture)
            : base(fixture)
        {
        }
    }
}
