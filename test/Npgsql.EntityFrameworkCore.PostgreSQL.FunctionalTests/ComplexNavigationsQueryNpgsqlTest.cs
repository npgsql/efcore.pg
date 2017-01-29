using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.ComplexNavigationsModel;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;
using Xunit.Abstractions;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class ComplexNavigationsQueryNpgsqlTest : ComplexNavigationsQueryTestBase<NpgsqlTestStore, ComplexNavigationsQueryNpgsqlFixture>
    {
        public ComplexNavigationsQueryNpgsqlTest(ComplexNavigationsQueryNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        [Fact(Skip="PostgreSQL sorts nulls first (#50)")]
        public override void OrderBy_nav_prop_reference_optional() {}

        [Fact(Skip= "PostgreSQL sorts nulls first (#50)")]
        public override void GroupJoin_on_left_side_being_a_subquery() {}

        [Fact(Skip = "PostgreSQL sorts nulls first (#50)")]
        public override void OrderBy_nav_prop_reference_optional_via_DefaultIfEmpty() {}

        [Fact(Skip = "PostgreSQL sorts nulls first (#50)")]
        public override void GroupJoin_on_right_side_being_a_subquery() { }
    }
}
