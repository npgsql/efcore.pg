using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.GearsOfWarModel;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
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
    }
}
