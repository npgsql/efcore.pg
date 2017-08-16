using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GearsOfWarQueryNpgsqlTest : GearsOfWarQueryTestBase<NpgsqlTestStore, GearsOfWarQueryNpgsqlFixture>
    {
        public GearsOfWarQueryNpgsqlTest(GearsOfWarQueryNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        [Fact(Skip= "PostgreSQL sorts nulls first (#50)")]
        public override void Optional_Navigation_Null_Coalesce_To_Clr_Type() {}

        [Fact(Skip = "PostgreSQL sorts nulls first (#50)")]
        public override void Include_with_nested_navigation_in_order_by() {}

        [Fact(Skip = "PostgreSQL sorts nulls first (#50)")]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2() {}

        [Fact(Skip = "PostgreSQL sorts nulls first (#50)")]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3() { }

        [Fact(Skip = "PostgreSQL sorts nulls first (#50)")]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result() { }

        [Fact(Skip = "PostgreSQL sorts nulls first (#50)")]
        public override void Select_null_propagation_negative3() {}

        [Fact(Skip = "PostgreSQL sorts nulls first (#50)")]
        public override void Select_null_propagation_negative4() { }

        [Fact(Skip = "PostgreSQL sorts nulls first (#50)")]
        public override void Select_null_propagation_negative5() { }
    }
}
