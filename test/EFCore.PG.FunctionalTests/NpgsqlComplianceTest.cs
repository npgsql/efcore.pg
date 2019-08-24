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
            // Spatial support disabled for now, waiting for NetTopologySuite 2.0
            typeof(SpatialTestBase<>),
            typeof(SpatialQueryTestBase<>),

            // TODO
            typeof(FromSqlSprocQueryTestBase<>)
        };

        protected override Assembly TargetAssembly { get; } = typeof(NpgsqlComplianceTest).Assembly;
    }
}
