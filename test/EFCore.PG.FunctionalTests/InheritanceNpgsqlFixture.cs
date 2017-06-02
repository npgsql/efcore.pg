using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests
{
    public class InheritanceNpgsqlFixture : InheritanceRelationalFixture
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        protected override void ClearLog()
        {
            TestSqlLoggerFactory.Clear();
        }

        public override DbContextOptions BuildOptions()
        {
            return
                new DbContextOptionsBuilder()
                    .EnableSensitiveDataLogging()
                    .UseNpgsql(
                        NpgsqlTestStore.CreateConnectionString("InheritanceNpgsqlTest"),
                        b => b.ApplyConfiguration())
                    .UseInternalServiceProvider(
                        new ServiceCollection()
                            .AddEntityFrameworkNpgsql()
                            .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                            .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                            .BuildServiceProvider())
                    .Options;
        }
    }
}
