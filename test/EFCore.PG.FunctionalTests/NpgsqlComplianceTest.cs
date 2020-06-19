using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class NpgsqlComplianceTest : RelationalComplianceTestBase
    {
        protected override ICollection<Type> IgnoredTestBases { get; } = new HashSet<Type>
        {
            // TODO
            typeof(FromSqlSprocQueryTestBase<>),
            // Split query tests had some issues at the point, will enable for preview7
            typeof(NorthwindSplitIncludeNoTrackingQueryTestBase<>),
            typeof(NorthwindSplitIncludeQueryTestBase<>)
        };

        protected override Assembly TargetAssembly { get; } = typeof(NpgsqlComplianceTest).Assembly;
    }
}
