using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GearsOfWarQueryNpgsqlTest : GearsOfWarQueryTestBase<GearsOfWarQueryNpgsqlFixture>
    {
        public GearsOfWarQueryNpgsqlTest(GearsOfWarQueryNpgsqlFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        [Fact(Skip = "In Test 2.1")]
        public override void Where_datetimeoffset_minute_component()
        {
            base.Where_datetimeoffset_minute_component();
        }

        [Fact(Skip = "In Test 2.1")]
        public override void Where_datetimeoffset_hour_component()
        {
            base.Where_datetimeoffset_minute_component();
        }

        [Fact(Skip = "PostgreSQL sorts nulls first (#50)")]
        public override void Optional_Navigation_Null_Coalesce_To_Clr_Type() { }

        [Fact(Skip = "PostgreSQL sorts nulls first (#50)")]
        public override void Include_with_nested_navigation_in_order_by() { }

        [Fact(Skip = "PostgreSQL sorts nulls first (#50)")]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2() { }

        [Fact(Skip = "PostgreSQL sorts nulls first (#50)")]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3() { }

        [Fact(Skip = "PostgreSQL sorts nulls first (#50)")]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result() { }

        [Fact(Skip = "PostgreSQL sorts nulls first (#50)")]
        public override void Select_null_propagation_negative3() { }

        [Fact(Skip = "PostgreSQL sorts nulls first (#50)")]
        public override void Select_null_propagation_negative4() { }

        [Fact(Skip = "PostgreSQL sorts nulls first (#50)")]
        public override void Select_null_propagation_negative5() { }
    }
}
