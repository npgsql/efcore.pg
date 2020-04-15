using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class MinimumPostgresVersionAttribute : Attribute, ITestCondition
    {
        readonly Version _version;

        public MinimumPostgresVersionAttribute(int major, int minor) => _version = new Version(major, minor);

        public ValueTask<bool> IsMetAsync() => new ValueTask<bool>(TestEnvironment.PostgresVersion >= _version);

        public string SkipReason => $"Requires PostgreSQL version {_version} or later.";
    }
}
