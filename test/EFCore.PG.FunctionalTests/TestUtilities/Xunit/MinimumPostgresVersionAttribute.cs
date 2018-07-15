using System;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities.Xunit
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class MinimumPostgresVersionAttribute : Attribute, ITestCondition
    {
        readonly Version _minVersion;

        public MinimumPostgresVersionAttribute(string minVersion)
            => _minVersion = new Version(minVersion);

        public MinimumPostgresVersionAttribute(int major, int minor)
            => _minVersion = new Version(major, minor);

        public MinimumPostgresVersionAttribute(int major, int minor, int build)
            => _minVersion = new Version(major, minor, build);

        public bool IsMet => TestEnvironment.PostgresVersion >= _minVersion;

        public string SkipReason =>
            $"Your PostgreSQL has version {TestEnvironment.PostgresVersion}, which below this test's minimum ({_minVersion})";
    }
}
