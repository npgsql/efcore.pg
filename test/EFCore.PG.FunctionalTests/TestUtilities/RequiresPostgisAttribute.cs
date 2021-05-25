using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class RequiresPostgisAttribute : Attribute, ITestCondition
    {
        public ValueTask<bool> IsMetAsync() => new(TestEnvironment.IsPostgisAvailable);

        public string SkipReason => $"Requires postgis.";
    }
}
