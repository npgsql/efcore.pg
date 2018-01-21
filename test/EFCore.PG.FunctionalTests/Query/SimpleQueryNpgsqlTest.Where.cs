using System;

namespace Microsoft.EntityFrameworkCore.Query
{
    public partial class SimpleQueryNpgsqlTest
    {
        public override void Where_string_indexof()
        {
            base.Where_string_indexof();

            throw new NotImplementedException();
            //AssertContainsSql("WHERE (STRPOS(\"c\".\"CompanyName\", 'ar') - 1) > 5");
        }
    }
}
