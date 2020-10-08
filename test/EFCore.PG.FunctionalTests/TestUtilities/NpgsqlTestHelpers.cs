using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Diagnostics.Internal;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities
{
    public class NpgsqlTestHelpers : TestHelpers
    {
        Version _postgresVersion;

        public class VersionScope : IDisposable
        {
            readonly NpgsqlTestHelpers _helpers;
            readonly Version _oldVersion, _newVersion;

            internal VersionScope(NpgsqlTestHelpers helpers, Version version)
            {
                _helpers = helpers;
                _oldVersion = helpers._postgresVersion;
                _newVersion = helpers._postgresVersion = version;
            }

            public void Dispose()
            {
                Assert.Equal(_helpers._postgresVersion, _newVersion);
                _helpers._postgresVersion = _oldVersion;
            }
        }

        public VersionScope WithPostgresVersion(Version version) => new VersionScope(this, version);

        protected NpgsqlTestHelpers() {}

        public static NpgsqlTestHelpers Instance { get; } = new NpgsqlTestHelpers();

        public override IServiceCollection AddProviderServices(IServiceCollection services)
            => services.AddEntityFrameworkNpgsql();

        public override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(new NpgsqlConnection("Host=localhost;Database=DummyDatabase"),
                   options => options.SetPostgresVersion(_postgresVersion));

        public override LoggingDefinitions LoggingDefinitions { get; } = new NpgsqlLoggingDefinitions();
    }
}
