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
            // These tests are ignored as of 3.0.0-preview7 - the list was copied from SqlServer
            typeof(ComplexNavigationsWeakQueryTestBase<>), // issue #15285
            typeof(FiltersInheritanceTestBase<>),          // issue #15264
            typeof(FiltersTestBase<>),                     // issue #15264
            typeof(OwnedQueryTestBase<>),                  // issue #15285
            typeof(QueryFilterFuncletizationTestBase<>),   // issue #15264
            typeof(RelationalOwnedQueryTestBase<>),        // issue #15285
            // Query pipeline
            typeof(ConcurrencyDetectorTestBase<>),
            typeof(CompiledQueryTestBase<>),
            typeof(InheritanceRelationshipsQueryTestBase<>),
            typeof(QueryNavigationsTestBase<>),
            typeof(ConcurrencyDetectorRelationalTestBase<>),
            typeof(GearsOfWarFromSqlQueryTestBase<>),
            typeof(QueryNoClientEvalTestBase<>),
            typeof(WarningsTestBase<>),

            // https://github.com/aspnet/EntityFrameworkCore/issues/15425
            typeof(UpdatesTestBase<>),
            typeof(UpdatesRelationalTestBase<>),

            // Spatial support punted to preview8
            typeof(SpatialTestBase<>),
            typeof(SpatialQueryTestBase<>),

            // The following are ignored because we haven't gotten around to doing them, not because they're
            // inherently not supported
            typeof(ComplexNavigationsWeakQueryTestBase<>),
            typeof(FunkyDataQueryTestBase<>),
            typeof(LoggingRelationalTestBase<,>),
            typeof(FromSqlSprocQueryTestBase<>)
        };

        protected override Assembly TargetAssembly { get; } = typeof(NpgsqlComplianceTest).Assembly;
    }
}
