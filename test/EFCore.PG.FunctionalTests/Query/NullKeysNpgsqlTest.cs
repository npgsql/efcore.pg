using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NullKeysNpgsqlTest : NullKeysTestBase<NullKeysNpgsqlTest.NullKeysNpgsqlFixture>
    {
        public NullKeysNpgsqlTest(NullKeysNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        public class NullKeysNpgsqlFixture : NullKeysFixtureBase, IDisposable
        {
            private readonly DbContextOptions _options;
            private readonly NpgsqlTestStore _testStore;

            public NullKeysNpgsqlFixture()
            {
                var name = "StringsContext";
                var connectionString = NpgsqlTestStore.CreateConnectionString(name);

                _options = new DbContextOptionsBuilder()
                    .UseNpgsql(connectionString, b => b.ApplyConfiguration())
                    .UseInternalServiceProvider(new ServiceCollection()
                        .AddEntityFrameworkNpgsql()
                        .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                        .BuildServiceProvider())
                    .Options;

                _testStore = NpgsqlTestStore.GetOrCreateShared(name, EnsureCreated);
            }

            public override DbContext CreateContext()
                => new DbContext(_options);

            public void Dispose() => _testStore.Dispose();
        }
    }
}
