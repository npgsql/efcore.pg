using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class NullKeysNpgsqlTest : NullKeysTestBase<NullKeysNpgsqlTest.NullKeysNpgsqlFixture>
    {
        public NullKeysNpgsqlTest(NullKeysNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public class NullKeysNpgsqlFixture : NullKeysFixtureBase
        {
            private readonly DbContextOptions _options;

            public NullKeysNpgsqlFixture()
            {
                _options = new DbContextOptionsBuilder()
                    .UseNpgsql(NpgsqlTestStore.CreateConnectionString("StringsContext"))
                    .UseInternalServiceProvider(new ServiceCollection()
                        .AddEntityFrameworkNpgsql()
                        .AddSingleton(TestNpgsqlModelSource.GetFactory(OnModelCreating))
                        .BuildServiceProvider())
                    .Options;

                EnsureCreated();
            }

            public override DbContext CreateContext()
                => new DbContext(_options);
        }
    }
}
