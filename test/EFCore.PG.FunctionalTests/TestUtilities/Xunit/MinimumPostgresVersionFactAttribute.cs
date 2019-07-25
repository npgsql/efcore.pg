using System;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities.Xunit
{
    public class MinimumPostgresVersionFactAttribute : FactAttribute
    {
        readonly Version _version;

        public override string Skip
        {
            get => base.Skip ?? (TestEnvironment.PostgresVersion < _version ? $"Requires PostgreSQL version {_version} or later." : null);
            set => base.Skip = value;
        }

        public MinimumPostgresVersionFactAttribute(int major, int minor) => _version = new Version(major, minor);
    }
}
