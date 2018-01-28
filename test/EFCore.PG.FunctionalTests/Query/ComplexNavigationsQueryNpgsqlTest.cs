using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ComplexNavigationsQueryNpgsqlTest
        : ComplexNavigationsQueryTestBase<ComplexNavigationsQueryNpgsqlFixture>
    {
        public ComplexNavigationsQueryNpgsqlTest(ComplexNavigationsQueryNpgsqlFixture fixture)
            : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        [Fact(Skip="PostgreSQL sorts nulls first (#50)")]
        public override void OrderBy_nav_prop_reference_optional() {}

        [Fact(Skip="PostgreSQL sorts nulls first (#50)")]
        public override void GroupJoin_on_left_side_being_a_subquery() {}

        [Fact(Skip="PostgreSQL sorts nulls first (#50)")]
        public override void OrderBy_nav_prop_reference_optional_via_DefaultIfEmpty() {}

        [Fact(Skip="PostgreSQL sorts nulls first (#50)")]
        public override void GroupJoin_on_right_side_being_a_subquery() {}

        [Fact(Skip="PostgreSQL sorts nulls first (#50)")]
        public override void Query_source_materialization_bug_4547() {}

        [Fact(Skip="PostgreSQL sorts nulls first (#50)")]
        public override void Optional_navigation_take_optional_navigation() {}

        [Fact(Skip="PostgreSQL sorts nulls first (#50)")]
        public override void Include_reference_collection_order_by_reference_navigation() {}
    }
}
