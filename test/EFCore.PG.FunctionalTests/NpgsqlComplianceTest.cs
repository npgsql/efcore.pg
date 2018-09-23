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
            // The following are ignored because we haven't gotten around to doing them, not because they're
            // inherently not supported
            typeof(ComplexNavigationsWeakQueryTestBase<>),
            typeof(FunkyDataQueryTestBase<>),
            typeof(LoggingRelationalTestBase<,>),
            typeof(AsyncFromSqlSprocQueryTestBase<>),
            typeof(FromSqlSprocQueryTestBase<>)
        };

        protected override Assembly TargetAssembly { get; } = typeof(NpgsqlComplianceTest).Assembly;
    }
}
