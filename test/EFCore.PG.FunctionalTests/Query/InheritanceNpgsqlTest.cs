using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class InheritanceNpgsqlTest : InheritanceTestBase<InheritanceNpgsqlFixture>
    {
        public InheritanceNpgsqlTest(InheritanceNpgsqlFixture fixture)
            : base(fixture)
        {
        }
    }
}
